using Microsoft.AspNetCore.Mvc;
using ShopEase.Application.Features.Categories;

namespace ShopEase.Web.Controllers;

public class CategoriesController : Controller
{
    private readonly CategoryService _categories;

    public CategoriesController(CategoryService categories)
    {
        _categories = categories;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        return View(await _categories.GetAllAsync(ct));
    }

    public IActionResult Create()
    {
        return View(new CategoryEditDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CategoryEditDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(dto);

        try
        {
            await _categories.CreateAsync(dto, ct);
            TempData["Success"] = "Category created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(dto.Name), ex.Message);
            return View(dto);
        }
    }

    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var category = await _categories.GetByIdAsync(id, ct);

        return category is null ? NotFound() : View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id, CategoryEditDto dto, CancellationToken ct)
    {
        if (id != dto.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(dto);

        try
        {
            if (!await _categories.UpdateAsync(dto, ct))
                return NotFound();

            TempData["Success"] = "Category updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(dto.Name), ex.Message);
            return View(dto);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        try
        {
            var deleted = await _categories.DeleteAsync(id, ct);

            TempData[deleted ? "Success" : "Error"] = deleted
                ? "Category deleted successfully."
                : "Category was not found.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}