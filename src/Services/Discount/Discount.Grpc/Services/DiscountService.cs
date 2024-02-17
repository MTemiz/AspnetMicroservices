using AutoMapper;
using Discount.Gprc.Repositories;
using Discount.Grpc.Entities;
using Discount.Grpc.Protos;
using Grpc.Core;

namespace Discount.Grpc.Services
{
    public class DiscountService : DiscountProtoService.DiscountProtoServiceBase
    {
        private readonly IDiscountRepository repository;
        private readonly IMapper mapper;
        private readonly ILogger<DiscountService> logger;

        public DiscountService(IDiscountRepository repository, ILogger<DiscountService> logger, IMapper mapper)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public override async Task<CouponModel> GetDiscount(GetDiscountRequest request, ServerCallContext context)
        {
            var coupon = await repository.GetDiscount(request.ProductName);

            if (coupon is null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"discount with productname {request.ProductName} is not found"));
            }

            logger.LogInformation($"discount is retrieved for ProductName : {coupon.ProductName} and Amount : {coupon.Amount}");

            return mapper.Map<CouponModel>(coupon);
        }

        public override async Task<CouponModel> CreateDiscount(CreateDiscountRequest request, ServerCallContext context)
        {
            var coupon = mapper.Map<Coupon>(request.Coupon);

            await repository.CreateDiscount(coupon);

            var couponModel = mapper.Map<CouponModel>(coupon);

            logger.LogInformation("Discount is successfully created. product name : {ProductName}", coupon.ProductName);

            return couponModel;
        }

        public override async Task<CouponModel> UppdateDiscount(UpdateDiscountRequest request, ServerCallContext context)
        {
            var coupon = mapper.Map<Coupon>(request.Coupon);

            await repository.UpdateDiscount(coupon);

            var couponModel = mapper.Map<CouponModel>(coupon);

            logger.LogInformation("Discount is successfully updated. product name : {ProductName}", coupon.ProductName);

            return couponModel;
        }

        public override async Task<DeleteDiscountResponse> DeleteDiscount(DeleteDiscountRequest request, ServerCallContext context)
        {
            DeleteDiscountResponse response = new DeleteDiscountResponse();

            response.Success = await repository.DeleteDiscount(request.ProductName);

            return response;
        }
    }
}
