using System;
using MediaRetentionGuardian.Services;
using Microsoft.AspNetCore.Mvc;

namespace MediaRetentionGuardian.Controllers;

/// <summary>
/// Provides drive information for the configuration UI.
/// </summary>
[ApiController]
[Route("MediaRetentionGuardian/DriveInfo")]
public class DriveInfoController : ControllerBase
{
    /// <summary>
    /// Gets the drive information for the provided path.
    /// </summary>
    /// <param name="path">The filesystem path.</param>
    /// <returns>Drive information for the path.</returns>
    [HttpGet]
    public ActionResult<DriveInfoResponse> Get(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return BadRequest("Path is required.");
        }

        try
        {
            var drive = RetentionCleanupService.TryResolveDrive(path);
            if (drive == null)
            {
                return NotFound();
            }

            if (!drive.IsReady || drive.TotalSize <= 0)
            {
                return new DriveInfoResponse
                {
                    RootPath = drive.RootDirectory.FullName
                };
            }

            return new DriveInfoResponse
            {
                RootPath = drive.RootDirectory.FullName,
                AvailableBytes = drive.AvailableFreeSpace,
                TotalBytes = drive.TotalSize
            };
        }
        catch (Exception ex)
        {
            return Problem(title: "Unable to resolve drive information.", detail: ex.Message);
        }
    }
}
