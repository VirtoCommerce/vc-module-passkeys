using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;
using VirtoCommerce.Passkeys.Core.Models;
using VirtoCommerce.Passkeys.Data.Services;
using VirtoCommerce.Platform.Core.Security;
using Xunit;

namespace VirtoCommerce.Passkeys.Tests;

[Trait("Category", "Unit")]
public class PasskeySignInMethodProviderTests
{
    // A real passkey attestation object, split at its AAGUID, comes from the MIT-licensed test file
    // https://github.com/passwordless-lib/fido2-net-lib/blob/master/Tests/Fido2.Tests/TestFiles/attestationResultsATKey.json
    private const string _attestationObjectBeforeAaguid = "o2NmbXRmcGFja2VkZ2F0dFN0bXSjY2FsZyZjc2lnWEcwRQIgJQqgE9K-R9dowj2fGNoa3-W8bEynvgZx0rbwJ27j4DQCIQDNGUY-M96nyKL_aaVTsouA_a4WH4ERfJtM8MR0l-ERZGN4NWOBWQIHMIICAzCCAaigAwIBAgIEAcAAADAKBggqhkjOPQQDAjBiMQswCQYDVQQGEwJTRTESMBAGA1UECgwJQVRLZXlDQTAwMSIwIAYDVQQLDBlBdXRoZW50aWNhdG9yIEF0dGVzdGF0aW9uMRswGQYDVQQDExJBdXRoZW50cmVuZCBDQSAwMDAwIBcNMjAwNTEyMTMzMzIwWhgPMjA2MDA1MDIxMzMzMjBaMGcxCzAJBgNVBAYTAlRXMRQwEgYDVQQKDAtBdXRoZW5UcmVuZDEiMCAGA1UECwwZQXV0aGVudGljYXRvciBBdHRlc3RhdGlvbjEeMBwGA1UEAwwVQVRLZXkuUHJvIFNOIDAxQzAwMDAwMFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAE02eZbXcVoWahCO1_zHaW8Qi6-QSAw4k3jUKvr8Fwxb75Pgc8PlujmUCfc1BfFY7Bj7Fw12kU7GViKbZT1omTIqNFMEMwEwYLKwYBBAGC5RwCAQEEBAQCBSAwIQYLKwYBBAGC5RwBAQQEEgQQ4alhg1AWTyS1W-OuI2FMxjAJBgNVHRMEAjAAMAoGCCqGSM49BAMCA0kAMEYCIQCJe3fgns8VFtgIN-pYASCq7ZYklqPjRol83P8oc5zqRgIhAK-yCOrHj6VCpKcTqdKQ7ZDjLctNg64SjPW093UhnfloaGF1dGhEYXRhWORJlg3liA6MaHQ0Fw9kdmBbj-SuuaKGMseZXPO6gx2XY0UAAAAM";
    private const string _attestationObjectAfterAaguid = "AGAXw_RF6wCA1xPnd0DzjnC7At3kgjtQRB199X-owQxsPuzSnEKZQPoWVJpto4E8vRSzySFUwBZFB9uvyQ0QZSD50ZnSnMZznHsaY5GGj2F59uKgU_EJPLnl0G0r8pUhikqlAQIDJiABIVggTRY8jqIY4mSwFChMN_VSdHJmszOGRHzOGutsbyeDGcMiWCCR7jByXUvXTa2Udh3ljVSiDWQunl1Yk6JMJYvdYOF1Pg";
    private const string _windowsHelloAaguid = "08987058-cadc-4b81-b6e1-30de50dcbe96";
    private const string _unknownAaguid = "11111111-1111-1111-1111-111111111111";

    private readonly List<UserPasskeyInfo> _passkeys = [];

    [Theory]
    [InlineData(_windowsHelloAaguid, "Windows Hello")]
    [InlineData(_unknownAaguid, null)]
    public async Task GetSignInMethodsAsync_Should_NameKeyByAaguid(string aaguid, string expectedName)
    {
        // Arrange
        var user = new ApplicationUser { Id = "user-id" };
        _passkeys.Add(BuildPasskey(aaguid));

        var provider = CreateProvider();

        // Act
        var signInMethods = await provider.GetSignInMethodsAsync(user);

        // Assert
        var key = Assert.Single(Assert.Single(signInMethods).Keys);
        Assert.Equal(expectedName, key.Name);
    }

    private static UserPasskeyInfo BuildPasskey(string aaguid)
    {
        byte[] attestationObject =
        [
            .. Base64Url.DecodeFromChars(_attestationObjectBeforeAaguid),
            .. Guid.Parse(aaguid).ToByteArray(bigEndian: true),
            .. Base64Url.DecodeFromChars(_attestationObjectAfterAaguid),
        ];

        return new UserPasskeyInfo(
            credentialId: [1],
            publicKey: [2],
            createdAt: DateTimeOffset.UtcNow,
            signCount: 0,
            transports: [],
            isUserVerified: true,
            isBackupEligible: true,
            isBackedUp: true,
            attestationObject,
            clientDataJson: [3]);
    }

    private PasskeySignInMethodProvider CreateProvider()
    {
        var userManager = new Mock<UserManager<ApplicationUser>>(Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

        userManager
            .Setup(x => x.GetPasskeysAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(_passkeys);

        return new PasskeySignInMethodProvider(Options.Create(new PasskeyOptions()), userManager.Object);
    }
}
