using Microsoft.AspNetCore.Mvc;
using LOi.DatabaseContext;
using LOi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using LOi.Validation;
using LOi.Services.AdminService;
using AutoMapper;

namespace LOi.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrderController : Controller
    {
        private readonly AppDbContext dbContext;
        private readonly IUserService _userService;
        private readonly IAdminService _adminService;
        private readonly IMapper mapper;
        private readonly OrderValidator orderValidator;
        private readonly ILogger<OrderController> logger;


        public OrderController(IUserService userService, IMapper mapper, IAdminService adminService, AppDbContext dbContext, ILogger<OrderController> logger)
        {
            this.dbContext = dbContext;
            _userService = userService;
            _adminService = adminService;
            orderValidator = new();
            this.logger = logger;
            this.mapper = mapper;
        }

        [HttpGet, Authorize(Roles = "User")]
        public async Task<IActionResult> GetOrders()
        {
            var Email = _userService.GetEmail();
            var User = await dbContext.UserDataTable.FirstOrDefaultAsync(x => x.Email == Email);
            var orders = await dbContext.OrderTable.Where(x => x.User == User).ToListAsync();
            logger.LogInformation($"User with Id {User.ID} viewed all orders");
            return Ok(orders);
        }


        public Dictionary<string, int> Prices = new()  //Dictionary mapping prices to ice cream sizes
        {
            {"100mL", 500},
            {"200mL", 1000},
            {"500mL", 2000},
            {"1000mL", 3800},
            {"2000mL", 7300},
        };


        [HttpPost, Authorize(Roles = "User")]
        public async Task<IActionResult> CreateOrder(OrderCreation orderCreation)
        {
            var Email = _userService.GetEmail();
            var User = await dbContext.UserDataTable.FirstOrDefaultAsync(x => x.Email == Email);

            var order = new Order()
            {
                OrderID = Guid.NewGuid(),
                Size = orderCreation.Size,
                Location = orderCreation.Location,
                Quantity = orderCreation.Quantity,
                OrderStatus = "Pending",
                CreationTime = DateTime.UtcNow.AddHours(1),
                UpdatedAt = DateTime.UtcNow.AddHours(1),
                Cost = orderCreation.Quantity * Prices[orderCreation.Size],
                User = User
            };

            var validatorResult = orderValidator.Validate(order);
            if (validatorResult.IsValid)
            {
                await dbContext.OrderTable.AddAsync(order);
                await dbContext.SaveChangesAsync();
                logger.LogInformation($"User with ID {User.ID} created an order with ID {order.OrderID} at {DateTime.UtcNow.AddHours(1)}");
                return Ok(order);
            }

            return StatusCode(StatusCodes.Status400BadRequest, validatorResult.Errors);
        }

        [HttpGet, Authorize]
        [Route("{OrderID}")]
        public async Task<IActionResult> ViewOrder([FromRoute] Guid OrderID) //FromRoute is not necessary
        {
            var order = await dbContext.OrderTable.FindAsync(OrderID);

            if (order != null)
            {
                var Email = _userService.GetEmail();
                var User = await dbContext.UserDataTable.FirstOrDefaultAsync(x => x.Email == Email);
                logger.LogInformation($"User with ID {User.ID} created an order with ID {order.OrderID} at {DateTime.UtcNow.AddHours(1)}");
                return Ok(order);
            }

            return NotFound();
        }

        [HttpPut, Authorize(Roles = "User")]
        [Route("{OrderID}")]

        public async Task<IActionResult> UpdateOrder(Guid OrderID, UpdateOrder updateOrder)
        {
            // return await dbContext.OrderTable.ToList();
            var order = await dbContext.OrderTable.FindAsync(OrderID);

            if (order != null)
            {
                order.Location = updateOrder.Location;
                order.Quantity = updateOrder.Quantity;
                order.Size = updateOrder.Size;
                order.UpdatedAt = DateTime.UtcNow.AddHours(1);
                order.Cost = updateOrder.Quantity * Prices[updateOrder.Size];

                var validatorResult = orderValidator.Validate(order);
                if (validatorResult.IsValid)
                {
                    await dbContext.SaveChangesAsync();
                    var Email = _userService.GetEmail();
                    var User = await dbContext.UserDataTable.FirstOrDefaultAsync(x => x.Email == Email);
                    logger.LogInformation($"User with ID {User.ID} updated order with ID {order.OrderID} at {DateTime.UtcNow.AddHours(1)}");
                    return Ok(order);
                }

                return StatusCode(StatusCodes.Status400BadRequest, validatorResult.Errors);
            }

            return NotFound();

        }

        [HttpDelete, Authorize]
        [Route("{OrderID}")]
        public async Task<IActionResult> DeleteOrder(Guid OrderID)
        {
            var order = await dbContext.OrderTable.FindAsync(OrderID);

            if (order != null)
            {
                dbContext.Remove(order);
                await dbContext.SaveChangesAsync();
                var Email = _userService.GetEmail();
                var User = await dbContext.UserDataTable.FirstOrDefaultAsync(x => x.Email == Email);
                logger.LogInformation($"Order with ID {order.OrderID} was deleted at {DateTime.UtcNow.AddHours(1)}");
                return Ok("Order was deleted successfully");
            }

            return NotFound();
        }

        [HttpPut, Authorize(Roles = "Admin")]
        [Route("admin/{OrderID}")]

        public async Task<IActionResult> AdminUpdateOrder(Guid OrderID, AdminUpdateOrder updateOrder)
        {
            // return await dbContext.OrderTable.ToList();
            var order = await dbContext.OrderTable.FindAsync(OrderID);

            if (order != null)
            {
                order.Location = updateOrder.Location;
                order.Quantity = updateOrder.Quantity;
                order.Size = updateOrder.Size;
                order.UpdatedAt = DateTime.UtcNow.AddHours(1);
                order.OrderStatus = updateOrder.OrderStatus;
                order.Cost = updateOrder.Quantity * Prices[updateOrder.Size];


                var validatorResult = orderValidator.Validate(order);
                if (validatorResult.IsValid)
                {
                    await dbContext.SaveChangesAsync();
                    var Name = _adminService.GetName();
                    var admin = await dbContext.AdminDataTable.FirstOrDefaultAsync(x => x.Name == Name);
                    logger.LogInformation($"Admin with ID {admin.ID} updated order with ID {order.OrderID} at {DateTime.UtcNow.AddHours(1)}");
                    return Ok(order);
                }

                return StatusCode(StatusCodes.Status400BadRequest, validatorResult.Errors);
            }

            return NotFound();

        }

        [HttpPost, Authorize(Roles = "Admin")]
        [Route("admin/create")]
        public async Task<IActionResult> AdminCreateOrder(AdminOrderCreation orderCreation)
        {
            var User = await dbContext.UserDataTable.FirstOrDefaultAsync(x => x.Email == orderCreation.Email);
            if (User != null)
            {
                var order = new Order()
                {
                    OrderID = Guid.NewGuid(),
                    Size = orderCreation.Size,
                    Location = orderCreation.Location,
                    Quantity = orderCreation.Quantity,
                    OrderStatus = "Pending",
                    CreationTime = DateTime.UtcNow.AddHours(1),
                    UpdatedAt = DateTime.UtcNow.AddHours(1),
                    Cost = orderCreation.Quantity * Prices[orderCreation.Size],
                    User = User
                };
                var validatorResult = orderValidator.Validate(order);
                if (validatorResult.IsValid)
                {
                    await dbContext.OrderTable.AddAsync(order);
                    await dbContext.SaveChangesAsync();
                    var Name = _adminService.GetName();
                    var admin = await dbContext.AdminDataTable.FirstOrDefaultAsync(x => x.Name == Name);
                    logger.LogInformation($"Admin with ID {admin.ID} created order with ID {order.OrderID} at {DateTime.UtcNow.AddHours(1)} for user with ID {User.ID}");
                    return Ok(order);
                }

                return StatusCode(StatusCodes.Status400BadRequest, validatorResult.Errors);
            }
            return BadRequest("User not found. Ivalid User ID.");
        }

        [HttpGet, Authorize(Roles = "Admin")]
        [Route("admin/all")]
        public async Task<IActionResult> AdminGetAllOrders()
        {
            var orders = await dbContext.OrderTable.ToListAsync();
            var Name = _adminService.GetName();
            var admin = await dbContext.AdminDataTable.FirstOrDefaultAsync(x => x.Name == Name);
            logger.LogInformation($"Admin with ID {admin.ID} viewed all orders at {DateTime.UtcNow.AddHours(1)}");
            return Ok(orders);
        }

        [HttpGet, Authorize(Roles = "Admin")]
        [Route("admin/{ID}")]
        public async Task<IActionResult> AdminGetUserOrders(Guid ID)
        {
            var User = await dbContext.UserDataTable.FindAsync(ID);
            if (User != null)
            {
                var orders = await dbContext.OrderTable.Where(x => x.User == User).ToListAsync();
                var Name = _adminService.GetName();
                var admin = await dbContext.AdminDataTable.FirstOrDefaultAsync(x => x.Name == Name);
                logger.LogInformation($"Admin with ID {admin.ID} viewed all orders placed by user with ID {User.ID} at {DateTime.UtcNow.AddHours(1)}");
                return Ok(orders);
            }

            return BadRequest("User not found. Ivalid User ID.");

        }
    }
}