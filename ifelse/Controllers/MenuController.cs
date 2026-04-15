using Microsoft.AspNetCore.Mvc;
using ifelse.Models;
using System.Runtime.InteropServices;
using ifelse.Data;

namespace ifelse.Controllers
{
    public class MenuController : Controller
    {
        // -- awal Defenisi variabel untuk menyimpan konteks database --
        private readonly AppDbContext _context; // buat menyimpan konteks database yang akan digunakan untuk mengakses data menu
        public MenuController(AppDbContext context) // buat konstruktor untuk menerima konteks database dari dependency injection dan menyimpannya ke variabel _context
        {
            _context = context;
        }
        // -- akhir Defenisi variabel untuk menyimpan konteks database --

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMenuAsync()
        {
            var menus = _context.Menu.ToList(); // ambil semua data menu dari database dan simpan ke variabel menus
            if (menus == null || menus.Count == 0) // cek apakah data menu kosong atau tidak
            {
                return NotFound(new { message = "Data menu tidak ditemukan" }); // jika kosong, kembalikan response 404 dengan pesan error
            }
            return View(menus); // jika tidak kosong, kembalikan view dengan data menu sebagai model
        }
        [HttpPost]
        public async Task<IActionResult> AddMenuAsync(MenuModel menu) // buat aksi untuk menambahkan menu baru dengan menerima data menu dari form
        {
            _context.Menu.Add(menu); // tambahkan data menu baru ke konteks database
            await _context.SaveChangesAsync(); // simpan perubahan ke database secara asynchronous
            return RedirectToAction("GetAllMenuAsync"); // setelah berhasil menambahkan, redirect ke aksi Get
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMenuAsync(int id, MenuModel menu) // buat aksi untuk mengupdate menu dengan menerima id menu dan data menu baru dari form
        {
            var existingMenu = await _context.Menu.FindAsync(id); // cari data menu yang akan diupdate berdasarkan id
            if (existingMenu == null) // cek apakah data menu ditemukan atau tidak
            {
                return NotFound(new { message = "Data menu tidak ditemukan" }); // jika tidak ditemukan, kembalikan response 404 dengan pesan error
            }
            if (ModelState.IsValid) // cek apakah data menu baru valid atau tidak
            {
                existingMenu.NamaMenu = menu.NamaMenu; // jika valid, update properti nama menu dengan data baru
                existingMenu.Harga = menu.Harga; // update properti harga dengan data baru
                existingMenu.Kategori = menu.Kategori; // update properti kategori dengan data baru
                existingMenu.Deskripsi = menu.Deskripsi; // update properti deskripsi dengan data baru
                existingMenu.IsAvailable = menu.IsAvailable; // update properti isAvailable dengan data baru
                await _context.SaveChangesAsync(); // simpan perubahan ke database secara asynchronous
                return RedirectToAction("GetAllMenuAsync"); // setelah berhasil mengupdate, redirect ke aksi Get
            }
            return View(menu); // jika data menu baru tidak valid, kembalikan view dengan data menu sebagai model
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMenuAsync(int id) // buat aksi untuk menghapus menu dengan menerima id menu
        {
            var existingMenu = await _context.Menu.FindAsync(id); // cari data menu yang akan dihapus berdasarkan id
            if (existingMenu == null) // cek apakah data menu ditemukan atau tidak
            {
                return NotFound(new { message = "Data menu tidak ditemukan" }); // jika tidak ditemukan, kembalikan response 404 dengan pesan error
            }
            _context.Menu.Remove(existingMenu); // jika ditemukan, hapus data menu dari konteks database
            await _context.SaveChangesAsync(); // simpan perubahan ke database secara asynchronous
            return RedirectToAction("GetAllMenuAsync"); // setelah berhasil menghapus, redirect ke aksi Get
        }
    }
}