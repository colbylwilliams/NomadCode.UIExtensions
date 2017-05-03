using System;
using System.Threading.Tasks;

#if __IOS__

using UIKit;
using Photos;

namespace NomadCode.UIExtensions
{
    public static class DialogExtensions
    {
		public static Task<T> ShowActionSheet<T> (this UIViewController vc, string title, string message, params T [] options)
		{
			var alertController = UIAlertController.Create (title, message, UIAlertControllerStyle.ActionSheet);
			var tcs = new TaskCompletionSource<T> ();

			foreach (var option in options)
			{
				alertController.AddAction (UIAlertAction.Create (option.ToString (), UIAlertActionStyle.Default, a => tcs.SetResult (option)));
			}

			alertController.AddAction (UIAlertAction.Create ("Cancel", UIAlertActionStyle.Cancel, a => tcs.SetResult (default (T))));

			vc.PresentViewController (alertController, true, null);

			return tcs.Task;
		}


		public static Task<bool> ShowTwoOptionAlert (this UIViewController vc, string title, string message, string yesText = "Yes", string noText = "No")
		{
			var alertController = UIAlertController.Create (title, message, UIAlertControllerStyle.Alert);
			var tcs = new TaskCompletionSource<bool> ();

			alertController.AddAction (UIAlertAction.Create (yesText, UIAlertActionStyle.Default, a => tcs.SetResult (true)));
			alertController.AddAction (UIAlertAction.Create (noText, UIAlertActionStyle.Cancel, a => tcs.SetResult (false)));

			vc.PresentViewController (alertController, true, null);

			return tcs.Task;
		}


		public static void ShowSimpleAlert (this UIViewController vc, string message, string title = "Hint", string okText = "Ok")
		{
			var alertController = UIAlertController.Create (title, message, UIAlertControllerStyle.Alert);
			alertController.AddAction (UIAlertAction.Create (okText, UIAlertActionStyle.Cancel, null));

			vc.PresentViewController (alertController, true, null);
		}


		public static Task ShowSimpleAlertWithWait (this UIViewController vc, string message, string title = "Hint", string okText = "Ok")
		{
			var tcs = new TaskCompletionSource<bool> ();

			var alertController = UIAlertController.Create (title, message, UIAlertControllerStyle.Alert);
			alertController.AddAction (UIAlertAction.Create (okText, UIAlertActionStyle.Cancel, a => tcs.SetResult (true)));

			vc.PresentViewController (alertController, true, null);

			return tcs.Task;
		}


        public async static Task<UIImage> ShowPhotoPicker (this UIViewController vc)
        {
            if (await CheckPhotoPermission ())
            {
                return await vc.ShowMediaPicker (UIImagePickerControllerSourceType.PhotoLibrary);
            }

            throw new Exception ("Need photo permission in order to pick photo");
        }


        public static Task<UIImage> ShowCameraPicker (this UIViewController vc)
        {
            return vc.ShowMediaPicker (UIImagePickerControllerSourceType.Camera);
        }


        public static Task<UIImage> ShowMediaPicker (this UIViewController vc, UIImagePickerControllerSourceType sourceType, bool allowEditing = true)
        {
            var tcs = new TaskCompletionSource<UIImage> ();

            var picker = new UIImagePickerController
            {
                SourceType = sourceType,
                MediaTypes = UIImagePickerController.AvailableMediaTypes (sourceType),
                AllowsEditing = allowEditing
            };

            picker.FinishedPickingMedia += (sender, e) =>
            {
                var image = e.EditedImage ?? e.OriginalImage;

                picker.DismissViewController (true, null);

                tcs.SetResult (image);
            };

            picker.Canceled += (sender, e) =>
            {
                picker.DismissViewController (true, null);
                tcs.SetResult (null);
            };

            vc.PresentViewController (picker, true, null);

            return tcs.Task;
        }


        public static Task<bool> CheckPhotoPermission ()
        {
            var tcs = new TaskCompletionSource<bool> ();
            var status = PHPhotoLibrary.AuthorizationStatus;

            switch (status)
            {
                case PHAuthorizationStatus.Authorized:
                    tcs.SetResult (true);
                    break;
                case PHAuthorizationStatus.Denied:
                case PHAuthorizationStatus.Restricted:
                    tcs.SetResult (false);
                    break;
                case PHAuthorizationStatus.NotDetermined:
                    PHPhotoLibrary.RequestAuthorization (newStatus =>
                    {
                        switch (newStatus)
                        {
                            case PHAuthorizationStatus.Authorized:
                                tcs.SetResult (true);
                                break;
                            case PHAuthorizationStatus.Denied:
                            case PHAuthorizationStatus.Restricted:
                            default:
                                tcs.SetResult (false);
                                break;
                        }
                    });
                    break;
            }

            return tcs.Task;
        }
    }
}

#endif