using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;

namespace Xamarin.BarcodeScanner.Android.Settings
{
    public class HeaderFooterTextLayoutApplier : ILayoutApplier
    {
        private readonly string _bottomText;
        private readonly string _topText;

        public HeaderFooterTextLayoutApplier(string topText, string bottomText)
        {
            _topText = topText;
            _bottomText = bottomText;
        }

        public void ApplyLayout(Context context, ViewGroup parent)
        {
            TextView topTextView = CreateScannerTextView(context, GravityFlags.Top);
            topTextView.Text = _topText;
            parent.AddView(topTextView);

            TextView bottomTextView = CreateScannerTextView(context, GravityFlags.Bottom);
            bottomTextView.Text = _bottomText;
            parent.AddView(bottomTextView);
        }

        private TextView CreateScannerTextView(Context context, GravityFlags gravity)
        {
            var textView = new TextView(context);

            var layoutParams = new FrameLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, gravity);
            var marginVertical = 196;
            layoutParams.SetMargins(0, marginVertical, 0, marginVertical);

            textView.LayoutParameters = layoutParams;
            textView.TextAlignment = TextAlignment.Center;
            textView.SetTextColor(Color.White);
            textView.TextSize = 16;

            return textView;
        }
    }
}