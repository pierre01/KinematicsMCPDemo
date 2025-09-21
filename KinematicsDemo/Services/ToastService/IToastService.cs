namespace KinematicsDemo.Services.ToastService;

public interface IToastService
{
    void ShowToast(string message, ToastLocation location, BadgeTypeEnum badgeType, int timeout, bool isClosable = true);
}