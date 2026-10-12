using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ShopEase.Application.Features.Categories;
using ShopEase.Application.Features.Products;
using ShopEase.Web.ViewModels;

namespace ShopEase.Web.Controllers;

public class ProductsController : Controller
{
    private readonly ProductService _products;
    private readonly CategoryService _categories;

    public ProductsController(
        ProductService products,
        CategoryService categories)
    {
        _products = products;
        _categories = categories;
    }

    public async Task<IActionResult> Index(
        string? search,
        int? categoryId,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _products.GetPageAsync(
            search,
            categoryId,
            page,
            pageSize,
            cancellationToken);

        return View(result);
    }

    public async Task<IActionResult> Details(
        int id,
        CancellationToken cancellationToken)
    {
        var product = await _products.GetByIdAsync(id, cancellationToken);

        if (product is null)
            return NotFound();

        return View(product);
    }

    public async Task<IActionResult> Create(
        CancellationToken cancellationToken)
    {
        var vm = new ProductFormViewModel();
        await PopulateCategories(vm, cancellationToken);

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        ProductFormViewModel vm,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateCategories(vm, cancellationToken);
            return View(vm);
        }

        try
        {
            await _products.CreateAsync(vm.Product, cancellationToken);
            TempData["Success"] = "Product created successfully.";

            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                nameof(vm.Product.CategoryId), ex.Message);

            await PopulateCategories(vm, cancellationToken);
            return View(vm);
        }
    }

    public async Task<IActionResult> Edit(
        int id,
        CancellationToken cancellationToken)
    {
        var product = await _products.GetByIdAsync(id, cancellationToken);

        if (product is null)
            return NotFound();

        var vm = new ProductFormViewModel
        {
            Product = new ProductEditDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                ImageUrl = product.ImageUrl
            }
        };

        vm.Product.CategoryId = product.CategoryId;

        await PopulateCategories(vm, cancellationToken);

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        ProductFormViewModel vm,
        CancellationToken cancellationToken)
    {
        if (id != vm.Product.Id)
            return BadRequest();

        if (!ModelState.IsValid)
        {
            await PopulateCategories(vm, cancellationToken);
            return View(vm);
        }

        try
        {
            var updated = await _products.UpdateAsync(
                vm.Product, cancellationToken);

            if (!updated)
                return NotFound();

            TempData["Success"] = "Product updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                nameof(vm.Product.CategoryId), ex.Message);

            await PopulateCategories(vm, cancellationToken);
            return View(vm);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var deleted = await _products.DeleteAsync(id, cancellationToken);

        TempData[deleted ? "Success" : "Error"] = deleted
            ? "Product deleted successfully."
            : "Product was not found.";

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateCategories(
        ProductFormViewModel vm,
        CancellationToken cancellationToken)
    {
        var categories = await _categories.GetAllAsync(cancellationToken);

        vm.Categories = categories.Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = c.Name
        });
    }
}