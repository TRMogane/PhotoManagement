﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using PhotoManagement.Models;
using PhotoManagement.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoManagement.Controllers
{
	[Authorize]
	public class HomeController : Controller
	{
		private readonly IAzureBlobService _azureBlobService;

		public HomeController(IAzureBlobService azureBlobService)
		{
			_azureBlobService = azureBlobService;
		}

		public async Task<ActionResult> Index()
		{
			try
			{
				var allBlobs = await _azureBlobService.ListAsync();
				return View(allBlobs);
			}
			catch (Exception ex)
			{
				ViewData["message"] = ex.Message;
				ViewData["trace"] = ex.StackTrace;
				return View("Error");
			}
		}

		[HttpPost]
		public async Task<ActionResult> UploadAsync()
		{
			try
			{
				var request = await HttpContext.Request.ReadFormAsync();
				if (request.Files == null)
				{
					return BadRequest("Could not upload files");
				}
				var files = request.Files;
				if (files.Count == 0)
				{
					return BadRequest("Could not upload empty files");
				}

				await _azureBlobService.UploadAsync(files);
				return RedirectToAction("Index");
			}
			catch (Exception ex)
			{
				ViewData["message"] = ex.Message;
				ViewData["trace"] = ex.StackTrace;
				return View("Error");
			}
		}

		[HttpPost]
		public async Task<ActionResult> DeleteImage(string fileUri)
		{
			try
			{
				await _azureBlobService.DeleteAsync(fileUri);
				return RedirectToAction("Index");
			}
			catch (Exception ex)
			{
				ViewData["message"] = ex.Message;
				ViewData["trace"] = ex.StackTrace;
				return View("Error");
			}
		}

		[HttpPost]
		public async Task<ActionResult> DeleteAll()
		{
			try
			{
				await _azureBlobService.DeleteAllAsync();
				return RedirectToAction("Index");
			}
			catch (Exception ex)
			{
				ViewData["message"] = ex.Message;
				ViewData["trace"] = ex.StackTrace;
				return View("Error");
			}
		}


		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}