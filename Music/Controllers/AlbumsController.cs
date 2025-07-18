﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Music.Data.Repositories.Interfaces;
using Music.Models;
using Music.Services.Interfaces;
using Music.ViewsModels;

namespace Music.Controllers
{
    [Authorize]
    public class AlbumsController : Controller
    {
        private readonly IAlbumRepository _context;
        private readonly IArtistRepository _artistRepository;
        private readonly IFavoriteService _favoriteService;
        private readonly IUserProvider _userProvider;
        private readonly string prefixKey = "Albums";

        public AlbumsController(IAlbumRepository context, IArtistRepository artistRepository
            , IFavoriteService favoriteService, IUserProvider userProvider)
        {
            _context = context;
            _artistRepository = artistRepository;
            _favoriteService = favoriteService;
            _favoriteService.AddCacheKeyPrefix(prefixKey);
            _userProvider = userProvider;
        }
        const int pageSize = 2;

        // GET: Albums
        public async Task<IActionResult> Index(int? idArtist = null, int page = 1)
        {
            if (page < 1)
            {
                page = 1;
            }
            
            ViewBag.UserFavoritesAlbums = await _favoriteService
                .GetUserFavoritesAlbumsAsync(_userProvider.GetCurrentUserId());
            if (idArtist != null)
            {
                var count = await _context.GetQuantityByArtist(idArtist);
                var pager = new PageViewModel(count, page);
                var skip = (page - 1) * pageSize;
                ViewBag.ArtistId = idArtist;
                var albums = await _context.GetAlbumsByArtist(idArtist, skip, pager.PageSize);
                ViewBag.Pager = pager;
                return View(albums);
            }
            else
            {
                var count = await _context.GetQuantity();
                var pager = new PageViewModel(count, page);
                var skip = (page - 1) * pageSize;
                var albums = await _context.GetPaginationAsync(skip, pager.PageSize);
                ViewBag.Pager = pager;
                return View(albums);
            }    
        }

        // GET: Albums/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var album = await _context.GetAlbumByIdAsync(id);
            if (album == null)
            {
                return NotFound();
            }

            return View(album);
        }

        [Authorize(Roles = "Admin")]
        //GET: Albums/Create
        public IActionResult Create(int id)
        {
            TempData["ArtistId"] = id;
            //ViewData["ArtistId"] ;
            return View();
        }

        [Authorize(Roles = "Admin")]
        // POST: Albums/Create
        [HttpPost]
        public async Task<IActionResult> Create([Bind("Name,YearOfIssue,UrlImg")] Album album)
        {
            album.ArtistId = (int)TempData["ArtistId"];
            var newAlbum = await _context.AddNewAlbumAsync(album);
            return RedirectToAction(nameof(Index),new {idArtist = newAlbum.ArtistId});
        }

        [Authorize(Roles = "Admin")]
        // GET: Albums/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var album = await _context.GetAlbumByIdAsync(id);
            if (album == null)
            {
                return NotFound();
            }
            SelectList list = new SelectList(_artistRepository.GetAllAsync().Result, "Id", "Name");
            ViewBag.Artists = list;
            return View(album);
        }

        [Authorize(Roles = "Admin")]
        // POST: Albums/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,YearOfIssue,UrlImg,ArtistId")] Album album)
        {
            if (id != album.Id)
            {
                return NotFound();
            }
            var newAlbum = await _context.UpdateAlbumAsync(album);
            return RedirectToAction(nameof(Index), new { idArtist = newAlbum.ArtistId });

            //ViewData["ArtistId"] = new SelectList(, "Id", "Id", album.ArtistId);
        }

        [Authorize(Roles = "Admin")]
        // GET: Albums/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var album = await _context.GetAlbumByIdAsync(id);
            if (album == null)
            {
                return NotFound();
            }

            return View(album);
        }

        [Authorize(Roles = "Admin")]
        // POST: Albums/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var idArtistDeletedAlbum = await _context.DeleteAlbumAsync(id);
            return RedirectToAction(nameof(Index), new { idArtist = idArtistDeletedAlbum });
        }


        // GET: Albums/id
        public async Task<IActionResult> AlbumsByArtist(int? id, int page = 1)
        {
            if (id == null)
            {
                return NotFound();
            }
            if (page < 1)
            {
                page = 1;
            }
            var count = await _context.GetQuantityByArtist(id);
            var pager = new PageViewModel(count, page);
            var skip = (page - 1) * pageSize;
            var albums = await _context.GetAlbumsByArtist(id, skip, pager.PageSize);
            if (albums == null)
            {
                return NotFound();
            }
            ViewBag.ArtistId = id;
            ViewBag.Pager = pager;
            return View("Index",albums);
        }
    }
}
