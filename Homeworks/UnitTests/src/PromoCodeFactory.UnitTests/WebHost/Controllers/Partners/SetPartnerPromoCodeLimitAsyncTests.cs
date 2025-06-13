using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.WebHost.Controllers;
using PromoCodeFactory.WebHost.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace PromoCodeFactory.UnitTests.WebHost.Controllers.Partners
{
    public class SetPartnerPromoCodeLimitAsyncTests
    {
        private readonly Mock<IRepository<Partner>> _partnersRepositoryMock;
        private readonly PartnersController _partnersController;
        private readonly SetPartnerPromoCodeLimitRequest _partnersPromoCodeLimitRequest;

        public SetPartnerPromoCodeLimitAsyncTests()
        {
            IFixture fixture = new Fixture().Customize(new AutoMoqCustomization());
            _partnersRepositoryMock = fixture.Freeze<Mock<IRepository<Partner>>>();
            _partnersController = fixture.Build<PartnersController>().OmitAutoProperties().Create();
            _partnersPromoCodeLimitRequest = fixture.Build<SetPartnerPromoCodeLimitRequest>()
                .With(r => r.EndDate, DateTime.Now.AddDays(new Random().Next(1, 365)))
                .Create();
        }

        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_PartnerIsNotFound_ReturnsNotFound()
        {
            // Arrange
            Guid partnerId = Guid.NewGuid();
            Partner partner = null;

            _partnersRepositoryMock.Setup(repo =>
                repo.GetByIdAsync(partnerId))
                .ReturnsAsync(partner);

            // Act
            var result = await _partnersController.SetPartnerPromoCodeLimitAsync(partnerId, _partnersPromoCodeLimitRequest);

            // Assert
            result.Should().BeAssignableTo<NotFoundResult>();
        }

        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_PartnerIsNotActive_ReturnsBadRequest()
        {
            // Arrange
            Partner partner = new PartnerBuilder()
                .WithActiveStatus(false)
                .Build();

            _partnersRepositoryMock.Setup(repo =>
                repo.GetByIdAsync(partner.Id))
                .ReturnsAsync(partner);

            // Act
            var result = await _partnersController.SetPartnerPromoCodeLimitAsync(partner.Id, _partnersPromoCodeLimitRequest);

            // Assert
            result.Should().BeAssignableTo<BadRequestObjectResult>();
        }


        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_WithActiveLimit_ResetsNumberIssuedPromoCodes()
        {
            // Arrange
            Partner partner = new PartnerBuilder()
                .WithIssuedPromoCodes(5)
                .WithActiveLimit()
                .Build();
            
            _partnersRepositoryMock.Setup(repo =>
                repo.GetByIdAsync(partner.Id))
                .ReturnsAsync(partner);

            // Act
            var result = await _partnersController.SetPartnerPromoCodeLimitAsync(partner.Id, _partnersPromoCodeLimitRequest);

            // Assert
            partner.NumberIssuedPromoCodes.Should().Be(0);
        }

        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_WithExpiredLimit_DoesNotResetNumberIssuedPromoCodes()
        {
            // Arrange
            int initialNumberIssuedPromoCodes = 30;

            Partner partner = new PartnerBuilder()
                .WithIssuedPromoCodes(initialNumberIssuedPromoCodes)
                .WithExpiredLimit(DateTime.Now.AddDays(-5))
                .Build();

            _partnersRepositoryMock.Setup(repo =>
                repo.GetByIdAsync(partner.Id))
                .ReturnsAsync(partner);

            // Act
            var result = await _partnersController.SetPartnerPromoCodeLimitAsync(partner.Id, _partnersPromoCodeLimitRequest);

            //// Assert
            partner.NumberIssuedPromoCodes.Should().Be(initialNumberIssuedPromoCodes);
        }


        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_WithActiveLimit_CancelDateHasValue()
        {
            // Arrange
            Guid partnerId = Guid.NewGuid();

            var limitBuilder = new PartnerPromoCodeLimitBuilder()
                .WithPartnerId(partnerId)
                .WithoutCancelDate();

            PartnerPromoCodeLimit limit = limitBuilder.Build();

            Partner partner = new PartnerBuilder()
                .WithId(partnerId)
                .WithLimit(limit)
                .Build();

            _partnersRepositoryMock.Setup(repo =>
                repo.GetByIdAsync(partnerId))
                .ReturnsAsync(partner);

            // Act
            var result = await _partnersController.SetPartnerPromoCodeLimitAsync(partnerId, _partnersPromoCodeLimitRequest);

            //// Assert
            limit.CancelDate.Should().NotBeNull();

        }

        [Theory]
        [InlineData(0)]
        [InlineData(-3)]
        public async Task SetPartnerPromoCodeLimitAsync_LimitEqualsOrLessThanZero_ReturnsBadRequest(int limit)
        {
            // Arrange
            Partner partner = new PartnerBuilder()
                .Build();

            _partnersRepositoryMock.Setup(repo =>
                repo.GetByIdAsync(partner.Id))
                .ReturnsAsync(partner);
            
            _partnersPromoCodeLimitRequest.Limit = limit;

            // Act
            var result = await _partnersController.SetPartnerPromoCodeLimitAsync(partner.Id, _partnersPromoCodeLimitRequest);

            // Assert
            result.Should().BeAssignableTo<BadRequestObjectResult>();
        }

        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_WithValidInitialConditions_SuccessfullyUpdatesDatabase()
        {
            // Arrange
            Partner partner = new PartnerBuilder()
                .WithIssuedPromoCodes(5)
                .WithActiveLimit()
                .Build();

            int initialPartnerLimitsCount = partner.PartnerLimits.Count;

            _partnersRepositoryMock.Setup(repo =>
                repo.GetByIdAsync(partner.Id))
                .ReturnsAsync(partner);

            // Act
            var result = await _partnersController.SetPartnerPromoCodeLimitAsync(partner.Id, _partnersPromoCodeLimitRequest);

            // Assert
            
            _partnersRepositoryMock.Verify(repo => repo.UpdateAsync(partner), Times.Once);
            
            result.Should().BeAssignableTo<CreatedAtActionResult>();

            partner.PartnerLimits.Count.Should().BeGreaterThan(initialPartnerLimitsCount);
        }

    }
}