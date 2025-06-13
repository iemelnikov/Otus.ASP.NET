using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using System;

namespace PromoCodeFactory.UnitTests.WebHost.Controllers.Partners
{
    internal class PartnerPromoCodeLimitBuilder
    {
        private readonly PartnerPromoCodeLimit _limit = new()
        {
            Id = Guid.NewGuid(),
            CreateDate = DateTime.Now.AddDays(-1),
            EndDate = DateTime.Now.AddDays(30),
            Limit = 100
        };

        public PartnerPromoCodeLimitBuilder WithPartnerId(Guid partnerId)
        {
            _limit.PartnerId = partnerId;
            return this;
        }

        public PartnerPromoCodeLimitBuilder WithEndDate(DateTime endDate)
        {
            _limit.EndDate = endDate;
            return this;
        }

        public PartnerPromoCodeLimitBuilder WithoutCancelDate()
        {
            _limit.CancelDate = null;
            return this;
        }

        public PartnerPromoCodeLimitBuilder WithCancelDate(DateTime? date = null)
        {
            _limit.CancelDate = date ?? DateTime.Now;
            return this;
        }

        public PartnerPromoCodeLimit Build() => _limit;
    }
}
