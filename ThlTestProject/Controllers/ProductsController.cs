using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThlTestProject.Models;

namespace ThlTestProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ProductContext _context;
        private readonly log4net.ILog log;
        public ProductsController(ProductContext context)
        {
            _context = context;
             log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }

        // GET: api/Products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            log.Info("Get all products.");
            return await _context.Products.ToListAsync();
        }

        // GET: api/Products/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Product>> GetProduct(long id)
        {
            log.Info("Get specific product by id.");
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        [HttpGet("{productName}")]
        public async Task<ActionResult<IEnumerable<Product>>> GetProduct(string productName)
        {
            log.Info("Get specific product by name. fuzzy search");
            if (string.IsNullOrEmpty(productName))
            {
                return await _context.Products.ToListAsync();
            }
            var products =  await _context.Products.Where(x => x.Name.ToLower().Contains(productName.ToLower())).ToListAsync();
            if(products.Count ==0)
            {
                var message = string.Format("No products found that contains '{0}' in it's name.", productName);
                return NotFound(message);
            }
            return products;
        }

        // PUT: api/Products/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(long id, Product product)
        {
            if (id != product.Id)
            {
                var message = string.Format("Invalid Product ID.");
                log.Debug(message);
                return BadRequest();
            }

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    var message = string.Format("Exception while updating, error : {0}",ex.Message);
                    log.Error(message);
                    throw;
                }
            }

            return Ok("Product updated successsfully.");
        }

        // POST: api/Products
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            
            if (string.IsNullOrEmpty(product.Name))
            {
                var message = string.Format("Product name cannot be empty.");
                log.Debug(message);
                return BadRequest(message);
            }
            if(!isProductUnique(product.Name))
            {
                var message = string.Format("Product {0} already exists.",product.Name);
                log.Debug(message);
                return BadRequest(message);
            }
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Product>> DeleteProduct(long id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                var message = string.Format("Product {0} does not exist.", product.Name);
                log.Debug(message);
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return product;
        }

        private bool ProductExists(long id)
        {
            return _context.Products.Any(e => e.Id == id);
        }

        private bool isProductUnique(string name)
        {
            return !_context.Products.Any(e => e.Name.ToLower() == name.ToLower());
        }
    }
}
