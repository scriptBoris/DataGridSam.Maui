﻿using DataGridSam.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGridSam
{
    public class CellTrigger : BindableObject, IDataTrigger
    {
        DataGrid? IDataTrigger.DataGrid { get; set; }
        public required BindingBase Binding { get; set; }
        public object? Value { get; set; }

        public int? CellTriggerId { get; set; }
        public object? CSharpValue { get; set; }

        #region bindable props
        // background color
        public static readonly BindableProperty BackgroundColorProperty = BindableProperty.Create(
            nameof(BackgroundColor),
            typeof(Color),
            typeof(CellTrigger),
            null,
            propertyChanged: Update
        );
        public Color? BackgroundColor
        {
            get => GetValue(BackgroundColorProperty) as Color;
            set => SetValue(BackgroundColorProperty, value);
        }

        // text color color
        public static readonly BindableProperty TextColorProperty = BindableProperty.Create(
            nameof(TextColor),
            typeof(Color),
            typeof(CellTrigger),
            null,
            propertyChanged: Update
        );
        public Color? TextColor
        {
            get => GetValue(TextColorProperty) as Color;
            set => SetValue(TextColorProperty, value);
        }

        // font size
        public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(
            nameof(FontSize),
            typeof(double?),
            typeof(CellTrigger),
            null,
            propertyChanged: Update
        );
        public double? FontSize
        {
            get => GetValue(FontSizeProperty) as double?;
            set => SetValue(FontSizeProperty, value);
        }

        // font attributes
        public static readonly BindableProperty FontAttributesProperty = BindableProperty.Create(
            nameof(FontAttributes),
            typeof(FontAttributes?),
            typeof(CellTrigger),
            null,
            propertyChanged: Update
        );
        public FontAttributes? FontAttributes
        {
            get => GetValue(FontAttributesProperty) as FontAttributes?;
            set => SetValue(FontAttributesProperty, value);
        }

        // vertical text alignment
        public static readonly BindableProperty VerticalTextAlignmentProperty = BindableProperty.Create(
            nameof(VerticalTextAlignment),
            typeof(TextAlignment?),
            typeof(CellTrigger),
            null,
            propertyChanged: Update
        );
        public TextAlignment? VerticalTextAlignment
        {
            get => GetValue(VerticalTextAlignmentProperty) as TextAlignment?;
            set => SetValue(VerticalTextAlignmentProperty, value);
        }

        // horizontal text alignment
        public static readonly BindableProperty HorizontalTextAlignmentProperty = BindableProperty.Create(
            nameof(HorizontalTextAlignment),
            typeof(TextAlignment?),
            typeof(CellTrigger),
            null,
            propertyChanged: Update
        );
        public TextAlignment? HorizontalTextAlignment
        {
            get => GetValue(HorizontalTextAlignmentProperty) as TextAlignment?;
            set => SetValue(HorizontalTextAlignmentProperty, value);
        }
        #endregion bindalbe props

        public static void Update(BindableObject b, object old, object newest)
        {
            if (b is IDataTrigger self)
            {
                self.DataGrid?.Draw();
            }
        }
    }
}