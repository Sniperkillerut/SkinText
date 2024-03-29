﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace SkinText {

    public class ResizingAdorner : Adorner {

        // Resizing adorner uses Thumbs for visual elements.
        // The Thumbs have built-in mouse input handling.
        private Thumb topLeft, topRight, bottomLeft, bottomRight;

        private readonly

                // To store and manage the adorner's visual children.
                VisualCollection visualChildren;

        // Initialize the ResizingAdorner.
        public ResizingAdorner(UIElement adornedElement)
            : base(adornedElement) {
            visualChildren = new VisualCollection(this);

            // Call a helper method to initialize the Thumbs
            // with a customized cursors.
            BuildAdornerCorner(ref topLeft, Cursors.SizeNWSE);
            BuildAdornerCorner(ref topRight, Cursors.SizeNESW);
            BuildAdornerCorner(ref bottomLeft, Cursors.SizeNESW);
            BuildAdornerCorner(ref bottomRight, Cursors.SizeNWSE);

            // Add handlers for resizing.
            bottomLeft.DragDelta += HandleBottomLeft;
            bottomRight.DragDelta += HandleBottomRight;
            topLeft.DragDelta += HandleTopLeft;
            topRight.DragDelta += HandleTopRight;
        }

        // Handler for resizing from the bottom-right.
        private void HandleBottomRight(object sender, DragDeltaEventArgs args) {
            FrameworkElement adornedElement = this.AdornedElement as FrameworkElement;
            Thumb hitThumb = sender as Thumb;

            if (adornedElement == null || hitThumb == null) {
                return;
            }

            FrameworkElement parentElement = adornedElement.Parent as FrameworkElement;

            // Ensure that the Width and Height are properly initialized after the resize.
            EnforceSize(adornedElement);

            // Change the size by the amount the user drags the mouse, as long as it's larger
            // than the width or height of an adorner, respectively.
            double newWidth = Math.Max(adornedElement.Width + args.HorizontalChange, hitThumb.DesiredSize.Width);

            if (newWidth > ((Canvas)adornedElement.Parent).ActualWidth - Canvas.GetLeft(adornedElement))
            {
                newWidth = ((Canvas)adornedElement.Parent).ActualWidth - Canvas.GetLeft(adornedElement);
            }
            adornedElement.Width = newWidth;

            double newHeight = Math.Max(args.VerticalChange + adornedElement.Height, hitThumb.DesiredSize.Height);

            if (newHeight > ((Canvas)adornedElement.Parent).ActualHeight - Canvas.GetTop(adornedElement))
            {
                newHeight = ((Canvas)adornedElement.Parent).ActualHeight - Canvas.GetTop(adornedElement);
            }
            adornedElement.Height = newHeight;
        }

        // Handler for resizing from the top-right.
        private void HandleTopRight(object sender, DragDeltaEventArgs args) {
            FrameworkElement adornedElement = this.AdornedElement as FrameworkElement;
            Thumb hitThumb = sender as Thumb;

            if (adornedElement == null || hitThumb == null) {
                return;
            }

            FrameworkElement parentElement = adornedElement.Parent as FrameworkElement;

            // Ensure that the Width and Height are properly initialized after the resize.
            EnforceSize(adornedElement);

            // Change the size by the amount the user drags the mouse, as long as it's larger
            // than the width or height of an adorner, respectively.
            double newWidth = Math.Max(adornedElement.Width + args.HorizontalChange, hitThumb.DesiredSize.Width);

            if (newWidth > ((Canvas)adornedElement.Parent).ActualWidth - Canvas.GetLeft(adornedElement))
            {
                newWidth = ((Canvas)adornedElement.Parent).ActualWidth - Canvas.GetLeft(adornedElement);
            }
            adornedElement.Width = newWidth;
            //adornedElement.Height = Math.Max(adornedElement.Height - args.VerticalChange, hitThumb.DesiredSize.Height);

            double height_old = adornedElement.Height;
            double height_new = Math.Max(adornedElement.Height - args.VerticalChange, hitThumb.DesiredSize.Height);
            double top_old = Canvas.GetTop(adornedElement);

            double newtop = top_old - (height_new - height_old);
            if (newtop < 0)
            {
                newtop = 0;
                height_new = height_old;
            }
            adornedElement.Height = height_new;
            Canvas.SetTop(adornedElement, newtop);
        }

        // Handler for resizing from the top-left.
        private void HandleTopLeft(object sender, DragDeltaEventArgs args) {
            FrameworkElement adornedElement = AdornedElement as FrameworkElement;
            Thumb hitThumb = sender as Thumb;

            if (adornedElement == null || hitThumb == null) {
                return;
            }

            // Ensure that the Width and Height are properly initialized after the resize.
            EnforceSize(adornedElement);

            // Change the size by the amount the user drags the mouse, as long as it's larger
            // than the width or height of an adorner, respectively.
            //adornedElement.Width = Math.Max(adornedElement.Width - args.HorizontalChange, hitThumb.DesiredSize.Width);
            //adornedElement.Height = Math.Max(adornedElement.Height - args.VerticalChange, hitThumb.DesiredSize.Height);

            double width_old = adornedElement.Width;
            double width_new = Math.Max(adornedElement.Width - args.HorizontalChange, hitThumb.DesiredSize.Width);
            double left_old = Canvas.GetLeft(adornedElement);

            double newleft = left_old - (width_new - width_old);
            if (newleft < 0)
            {
                newleft = 0;
                width_new = width_old;
            }
            adornedElement.Width = width_new;
            Canvas.SetLeft(adornedElement, newleft);

            double height_old = adornedElement.Height;
            double height_new = Math.Max(adornedElement.Height - args.VerticalChange, hitThumb.DesiredSize.Height);
            double top_old = Canvas.GetTop(adornedElement);

            double newtop = top_old - (height_new - height_old);
            if (newtop < 0)
            {
                newtop = 0;
                height_new = height_old;
            }
            adornedElement.Height = height_new;
            Canvas.SetTop(adornedElement, newtop);
        }

        // Handler for resizing from the bottom-left.
        private void HandleBottomLeft(object sender, DragDeltaEventArgs args) {
            FrameworkElement adornedElement = AdornedElement as FrameworkElement;
            Thumb hitThumb = sender as Thumb;

            if (adornedElement == null || hitThumb == null) {
                return;
            }

            // Ensure that the Width and Height are properly initialized after the resize.
            EnforceSize(adornedElement);

            // Change the size by the amount the user drags the mouse, as long as it's larger
            // than the width or height of an adorner, respectively.
            //adornedElement.Width = Math.Max(adornedElement.Width - args.HorizontalChange, hitThumb.DesiredSize.Width);
            double newHeight = Math.Max(args.VerticalChange + adornedElement.Height, hitThumb.DesiredSize.Height);

            if (newHeight> ((Canvas)adornedElement.Parent).ActualHeight - Canvas.GetTop(adornedElement))
            {
                newHeight = ((Canvas)adornedElement.Parent).ActualHeight - Canvas.GetTop(adornedElement);
            }
            adornedElement.Height = newHeight;

            double width_old = adornedElement.Width;
            double width_new = Math.Max(adornedElement.Width - args.HorizontalChange, hitThumb.DesiredSize.Width);

            double left_old = Canvas.GetLeft(adornedElement);

            double newleft = left_old - (width_new - width_old);
            if (newleft<0)
            {
                newleft = 0;
                width_new = width_old;
            }
            adornedElement.Width = width_new;
            Canvas.SetLeft(adornedElement, newleft);
        }

        // Arrange the Adorners.
        protected override Size ArrangeOverride(Size finalSize) {
            // desiredWidth and desiredHeight are the width and height of the element that's being adorned.
            // These will be used to place the ResizingAdorner at the corners of the adorned element.
            double desiredWidth = AdornedElement.DesiredSize.Width;
            double desiredHeight = AdornedElement.DesiredSize.Height;
            // adornerWidth & adornerHeight are used for placement as well.
            double adornerWidth = this.DesiredSize.Width;
            double adornerHeight = this.DesiredSize.Height;

            topLeft.Arrange(new Rect(-adornerWidth / 2, -adornerHeight / 2, adornerWidth, adornerHeight));
            topRight.Arrange(new Rect(desiredWidth - adornerWidth / 2, -adornerHeight / 2, adornerWidth, adornerHeight));
            bottomLeft.Arrange(new Rect(-adornerWidth / 2, desiredHeight - adornerHeight / 2, adornerWidth, adornerHeight));
            bottomRight.Arrange(new Rect(desiredWidth - adornerWidth / 2, desiredHeight - adornerHeight / 2, adornerWidth, adornerHeight));

            // Return the final size.
            return finalSize;
        }

        // Helper method to instantiate the corner Thumbs, set the Cursor property,
        // set some appearance properties, and add the elements to the visual tree.
        private void BuildAdornerCorner(ref Thumb cornerThumb, Cursor customizedCursor) {
            if (cornerThumb != null) {
                return;
            }

            cornerThumb = new Thumb {
                // Set some arbitrary visual characteristics.
                Cursor = customizedCursor,
                Opacity = 0.40,
                Background = new SolidColorBrush(Colors.MediumBlue),
                Height = 10,
                Width = 10
            };

            visualChildren.Add(cornerThumb);
        }

        // This method ensures that the Widths and Heights are initialized.  Sizing to content produces
        // Width and Height values of Double.NaN.  Because this Adorner explicitly resizes, the Width and Height
        // need to be set first.  It also sets the maximum size of the adorned element.
        private void EnforceSize(FrameworkElement adornedElement) {
            if (adornedElement.Width.Equals(double.NaN)) {
                adornedElement.Width = adornedElement.DesiredSize.Width;
            }

            if (adornedElement.Height.Equals(double.NaN)) {
                adornedElement.Height = adornedElement.DesiredSize.Height;
            }

            FrameworkElement parent = adornedElement.Parent as FrameworkElement;
            if (parent != null) {
                adornedElement.MaxHeight = parent.ActualHeight;
                adornedElement.MaxWidth = parent.ActualWidth;
            }
        }

        // Override the VisualChildrenCount and GetVisualChild properties to interface with
        // the adorner's visual collection.
        protected override int VisualChildrenCount => visualChildren.Count;

        protected override Visual GetVisualChild(int index) => visualChildren[index];
    }
}