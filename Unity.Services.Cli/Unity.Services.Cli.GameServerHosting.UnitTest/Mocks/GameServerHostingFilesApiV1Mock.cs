using Moq;
using Unity.Services.Gateway.GameServerHostingApiV1.Generated.Api;
using Unity.Services.Gateway.GameServerHostingApiV1.Generated.Model;
using File = Unity.Services.Gateway.GameServerHostingApiV1.Generated.Model.File;
using Unity.Services.Gateway.GameServerHostingApiV1.Generated.Client;

namespace Unity.Services.Cli.GameServerHosting.UnitTest.Mocks;

public class GameServerHostingFilesApiV1Mock
{
    readonly List<File> m_TestFiles = new()
    {
        new File(
            filename: "server.log",
            path: "logs/",
            fileSize: 100,
            createdAt: new DateTime(2022, 10, 11),
            lastModified: new DateTime(2022, 10, 12),
            fleet: new FleetDetails(
                id: new Guid(ValidFleetId),
                name: "Test Fleet"
            ),
            machine: new Machine(
                id: ValidMachineId,
                location: "europe-west1"
            ),
            serverID: ValidServerId
        ),
        new File(
            filename: "error.log",
            path: "logs/",
            fileSize: 100,
            createdAt: new DateTime(2022, 10, 11),
            lastModified: new DateTime(2022, 10, 12),
            fleet: new FleetDetails(
                id: new Guid(ValidFleetId),
                name: "Test Fleet"
            ),
            machine: new Machine(
                id: ValidMachineId,
                location: "europe-west1"
            ),
            serverID: ValidServerId2
        )
    };

    public Mock<IFilesApi> DefaultFilesClient = new();

    public List<Guid>? ValidEnvironments;

    public List<Guid>? ValidProjects;
    public void SetUp()
    {
        DefaultFilesClient = new Mock<IFilesApi>();
        DefaultFilesClient.Setup(a => a.Configuration)
            .Returns(new Configuration());

        DefaultFilesClient.Setup(
                a => a.ListFilesAsync(
                    It.IsAny<Guid>(), // projectId
                    It.IsAny<Guid>(), // environmentId
                    It.IsAny<FilesListRequest>(),
                    0,
                    CancellationToken.None
                ))
            .Returns(
                (
                    Guid projectId,
                    Guid environmentId,
                    FilesListRequest filesListRequest,
                    int _,
                    CancellationToken _
                ) =>
                {
                    var validated = ValidateProjectEnvironment(projectId, environmentId);
                    if (!validated) throw new HttpRequestException();

                    var results = m_TestFiles.AsEnumerable();

                    if (filesListRequest.PathFilter != null)
                    {
                        results = results.Where(a => a.Filename.Contains(filesListRequest.PathFilter));
                    }

                    if (filesListRequest.ServerIds != null)
                    {
                        results = results.Where(a => filesListRequest.ServerIds.Contains(a.ServerID));
                    }

                    if (filesListRequest.ModifiedFrom != null)
                    {
                        results = results.Where(a => ValidateDateAfterFromString(a.LastModified, filesListRequest.ModifiedFrom));

                    }

                    if (filesListRequest.ModifiedTo != null)
                    {
                        results = results.Where(a => ValidateDateBeforeFromString(a.LastModified, filesListRequest.ModifiedTo));

                    }

                    if (filesListRequest.Limit > 0)
                    {
                        results = results.Take((int)filesListRequest.Limit);
                    }

                    return Task.FromResult(results.ToList());
                }
            );
    }

    static bool ValidateDateBeforeFromString(DateTime lastModified, DateTime modifiedTo)
    {
        // providedDate is before targetDate
        if (lastModified < modifiedTo || lastModified == modifiedTo) return true;
        return false;
    }

    static bool ValidateDateAfterFromString(DateTime lastModified, DateTime modifiedFrom)
    {
        // providedDate is after targetDate
        if (lastModified > modifiedFrom || lastModified == modifiedFrom) return true;
        return false;
    }

    bool ValidateProjectEnvironment(Guid projectId, Guid environmentId)
    {
        if (ValidProjects != null && !ValidProjects.Contains(projectId)) return false;
        if (ValidEnvironments != null && !ValidEnvironments.Contains(environmentId)) return false;
        return true;
    }
}
