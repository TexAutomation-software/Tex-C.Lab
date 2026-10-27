using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GZSoft.Tex.Controller;
using GZSoft.Tex.Controller.Interface;
using PCRA.Services;
using System;
using System.Linq;
using System.IO;

namespace PCRA.ViewModels.Pages;

public partial class GeneratorsViewModel : ViewModelBase
{
    private readonly IController _controller;
    private readonly FileGeneratorService _fileGeneratorService;
    private readonly AppSettingsService _appSettingsService;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GenerateLoginCommand))]
    private string? loginPassword;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GenerateChangePasswordCommand))]
    private string? changePassword;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(
        nameof(GenerateResetCommand),
        nameof(SetResetTenMinutesCommand),
        nameof(SetResetOneHourCommand),
        nameof(SetResetOneDayCommand))]

    private string? macAddressReset;


    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GenerateResetCommand))]
    private TimeSpan? resetTime;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GenerateResetCommand))]
    private DateTimeOffset? resetDate;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GenerateCardParsCommand))]
    private string? cardParsFile;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GenerateChecksumCommand))]
    private string? checksumFile;


    private const string CardParsExtension = ".CRD";
    private const string ChecksumExtension = ".BIN";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(
        nameof(GenerateTempAuthCommand),
        nameof(SetTempAuthTenMinutesCommand),
        nameof(SetTempAuthOneHourCommand),
        nameof(SetTempAuthOneDayCommand))]
    private string? macAddressTempAuth;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GenerateTempAuthCommand))]
    private TimeSpan? tempAuthTime;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GenerateTempAuthCommand))]
    private DateTimeOffset? tempAuthDate;


    public GeneratorsViewModel(AppSettingsService appSettingsService)
    {
        _controller = ControllerDetectionHelper.CreateInstance(Board.PowerS);
        _fileGeneratorService = new FileGeneratorService(appSettingsService);
        _appSettingsService = appSettingsService;

        var now = DateTimeOffset.Now;

        ResetDate = new DateTimeOffset(
            now.Year,
            now.Month,
            now.Day,
            0,
            0,
            0,
            now.Offset);

        ResetTime = now.TimeOfDay;
        TempAuthDate = ResetDate;
        TempAuthTime = ResetTime;
    }

    [RelayCommand(CanExecute = nameof(CanGenerateLogin))]
    private void GenerateLogin()
    {
        if (string.IsNullOrWhiteSpace(LoginPassword))
            return;

        _fileGeneratorService.GenerateLoginFile(
            _controller,
            LoginPassword);
    }

    [RelayCommand(CanExecute = nameof(CanGenerateChangePassword))]
    private void GenerateChangePassword()
    {
        if (string.IsNullOrWhiteSpace(ChangePassword))
            return;

        _fileGeneratorService.GenerateChangePasswordFile(
            _controller,
            ChangePassword);
    }

    [RelayCommand(CanExecute = nameof(CanGenerateReset))]
    private void GenerateReset()
    {
        if (!TryParseMacAddress(macAddressReset, out byte[] mac))
            return;

        if (!TryGetResetTimestamp(out long unixTimestamp))
            return;

        _fileGeneratorService.GenerateResetPasswordFile(
            _controller,
            mac,
            unixTimestamp);
    }

    [RelayCommand(CanExecute = nameof(CanSetResetExpiration))]
    private void SetResetTenMinutes()
    {
        SetResetExpiration(TimeSpan.FromMinutes(10));
    }

    [RelayCommand(CanExecute = nameof(CanSetResetExpiration))]
    private void SetResetOneHour()
    {
        SetResetExpiration(TimeSpan.FromHours(1));
    }

    [RelayCommand(CanExecute = nameof(CanSetResetExpiration))]
    private void SetResetOneDay()
    {
        SetResetExpiration(TimeSpan.FromDays(1));
    }

    private bool CanSetResetExpiration()
    {
        return TryParseMacAddress(macAddressReset, out _);
    }

    private void SetResetExpiration(TimeSpan duration)
    {
        DateTimeOffset expiration =
            DateTimeOffset.Now.Add(duration);

        ResetDate = expiration.Date;
        ResetTime = expiration.TimeOfDay;
    }

    [RelayCommand(CanExecute = nameof(CanGenerateCardPars))]
    private void GenerateCardPars()
    {
        if (string.IsNullOrWhiteSpace(CardParsFile))
            return;

        _fileGeneratorService.GenerateCardParsFile(
            _controller,
            CardParsFile);
    }

    [RelayCommand(CanExecute = nameof(CanGenerateChecksum))]
    private void GenerateChecksum()
    {
        if (string.IsNullOrWhiteSpace(ChecksumFile))
            return;

        _fileGeneratorService.GenerateChecksumFile(
            _controller,
            ChecksumFile);
    }

    private bool CanGenerateLogin()
    {
        return IsValidPassword(LoginPassword);
    }

    private bool CanGenerateChangePassword()
    {
        return IsValidPassword(ChangePassword);
    }

    private static bool IsValidPassword(string? password)
    {
        return !string.IsNullOrWhiteSpace(password) &&
               password.Length >= 8 &&
               password.Length <= 32;
    }

    private static bool TryParseMacAddress(string? value,out byte[] mac)
    {
        mac = Array.Empty<byte>();

        if (string.IsNullOrWhiteSpace(value))
            return false;

        string cleaned = value
            .Replace(":", "")
            .Replace("-", "")
            .Trim();

        // A MAC address must contain exactly 12 hexadecimal characters.
        if (cleaned.Length != 12 ||
            !cleaned.All(Uri.IsHexDigit))
        {
            return false;
        }

        try
        {
            mac = Enumerable
                .Range(0, 6)
                .Select(i => Convert.ToByte(cleaned.Substring(i * 2, 2), 16))
                .ToArray();

            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }


    private bool TryGetResetTimestamp(out long unixTimestamp)
    {
        unixTimestamp = 0;

        if (!TryParseMacAddress(macAddressReset, out _))
            return false;

        if (ResetDate is null || ResetTime is null)
            return false;

        /*
         * DatePicker and TimePicker represent the user's local wall-clock
         * selection. Treat the combined value as local time, then convert
         * it to UTC.
         */
        DateTime localDateTime = DateTime.SpecifyKind(
            ResetDate.Value.Date + ResetTime.Value,
            DateTimeKind.Unspecified);

        TimeSpan localOffset =
            TimeZoneInfo.Local.GetUtcOffset(localDateTime);

        DateTimeOffset selectedLocalTime =
            new DateTimeOffset(localDateTime, localOffset);

        if (selectedLocalTime <= DateTimeOffset.Now)
            return false;

        unixTimestamp = selectedLocalTime
            .ToUniversalTime()
            .ToUnixTimeSeconds();

        return true;
    }

    private bool CanGenerateReset()
    {
        return TryGetResetTimestamp(out _);
    }

    private static bool HasExtension(string? path,string expectedExtension)
    {
        return !string.IsNullOrWhiteSpace(path) &&
               File.Exists(path) && 
               string.Equals(Path.GetExtension(path),expectedExtension,StringComparison.OrdinalIgnoreCase);
    }

    private bool CanGenerateCardPars()
    {
        return HasExtension(CardParsFile, CardParsExtension);
    }

    private bool CanGenerateChecksum()
    {
        return HasExtension(ChecksumFile, ChecksumExtension);
    }

    [RelayCommand(CanExecute = nameof(CanGenerateTempAuth))]
    private void GenerateTempAuth()
    {
        if (!TryParseMacAddress(MacAddressTempAuth, out byte[] mac))
            return;

        if (!TryGetTempAuthTimestamp(out long unixTimestamp))
            return;

        _fileGeneratorService.GenerateTempAuthFile(
            _controller,
            mac,
            unixTimestamp);
    }

    [RelayCommand(CanExecute = nameof(CanSetTempAuthExpiration))]
    private void SetTempAuthTenMinutes()
    {
        SetTempAuthExpiration(TimeSpan.FromMinutes(10));
    }

    [RelayCommand(CanExecute = nameof(CanSetTempAuthExpiration))]
    private void SetTempAuthOneHour()
    {
        SetTempAuthExpiration(TimeSpan.FromHours(1));
    }

    [RelayCommand(CanExecute = nameof(CanSetTempAuthExpiration))]
    private void SetTempAuthOneDay()
    {
        SetTempAuthExpiration(TimeSpan.FromDays(1));
    }

    private bool CanSetTempAuthExpiration()
    {
        return TryParseMacAddress(MacAddressTempAuth, out _);
    }

    private void SetTempAuthExpiration(TimeSpan duration)
    {
        DateTimeOffset expiration = DateTimeOffset.Now.Add(duration);

        TempAuthDate = expiration.Date;
        TempAuthTime = expiration.TimeOfDay;
    }

    private bool TryGetTempAuthTimestamp(out long unixTimestamp)
    {
        unixTimestamp = 0;

        if (!TryParseMacAddress(MacAddressTempAuth, out _))
            return false;

        if (TempAuthDate is null || TempAuthTime is null)
            return false;

        DateTime localDateTime = DateTime.SpecifyKind(
            TempAuthDate.Value.Date + TempAuthTime.Value,
            DateTimeKind.Unspecified);

        TimeSpan localOffset =
            TimeZoneInfo.Local.GetUtcOffset(localDateTime);

        DateTimeOffset selectedLocalTime =
            new DateTimeOffset(localDateTime, localOffset);

        if (selectedLocalTime <= DateTimeOffset.Now)
            return false;

        unixTimestamp = selectedLocalTime
            .ToUniversalTime()
            .ToUnixTimeSeconds();

        return true;
    }

    private bool CanGenerateTempAuth()
    {
        return TryGetTempAuthTimestamp(out _);
    }

}