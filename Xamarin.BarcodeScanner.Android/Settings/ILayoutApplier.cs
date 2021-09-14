using Android.Content;
using Android.Views;

namespace Xamarin.BarcodeScanner.Android.Settings
{
    public interface ILayoutApplier
    {
        void ApplyLayout(Context context, ViewGroup parent);
    }
}