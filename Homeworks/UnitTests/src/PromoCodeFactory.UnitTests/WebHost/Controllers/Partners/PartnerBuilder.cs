using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using System;

namespace PromoCodeFactory.UnitTests.WebHost.Controllers.Partners
{
    internal class PartnerBuilder
    {
        private readonly Partner _partner = new()
        {
            Id = Guid.NewGuid(),
            Name = "Test Partner",
            IsActive = true,
            NumberIssuedPromoCodes = 0,
            PartnerLimits = []
        };

        public PartnerBuilder WithId(Guid id)
        {
            _partner.Id = id;
            return this;
        }

        public PartnerBuilder WithActiveStatus(bool isActive)
        {
            _partner.IsActive = isActive;
            return this;
        }

        public PartnerBuilder WithIssuedPromoCodes(int count)
        {
            _partner.NumberIssuedPromoCodes = count;
            return this;
        }

        public PartnerBuilder WithLimit(PartnerPromoCodeLimit limit)
        {
            _partner.PartnerLimits.Add(limit);
            return this;
        }

        public PartnerBuilder WithActiveLimit()
        {
            var limitBuilder = new PartnerPromoCodeLimitBuilder()
                .WithPartnerId(_partner.Id)
                .WithoutCancelDate();

            _partner.PartnerLimits.Add(limitBuilder.Build());
            return this;
        }

        public PartnerBuilder WithExpiredLimit(DateTime? expiredDate = null)
        {
            var limitBuilder = new PartnerPromoCodeLimitBuilder()
                .WithPartnerId(_partner.Id)
                .WithCancelDate(expiredDate);

            _partner.PartnerLimits.Add(limitBuilder.Build());
            return this;
        }

        public Partner Build() => _partner;
    }
}
