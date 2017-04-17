﻿using System;
using System.ComponentModel;
using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Xenko.UI;

namespace SiliconStudio.Xenko.Assets.UI
{
    /// <summary>
    /// Associate an <see cref="UIElement"/> with design-time data.
    /// </summary>
    [DataContract("UIElementDesign")]
    public class UIElementDesign : IAssetPartDesign<UIElement>, IEquatable<UIElementDesign>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="UIElementDesign"/>.
        /// </summary>
        /// <remarks>
        /// This constructor is used only for serialization.
        /// </remarks>
        public UIElementDesign()
            // ReSharper disable once AssignNullToNotNullAttribute
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="UIElementDesign"/>.
        /// </summary>
        /// <param name="uiElement">The UI Element</param>
        public UIElementDesign([NotNull] UIElement uiElement)
        {
            UIElement = uiElement;
        }

        /// <summary>
        /// The UI element.
        /// </summary>
        [DataMember(10)]
        [NotNull]
        public UIElement UIElement { get; set; }

        /// <inheritdoc/>
        [DataMember(20)]
        [DefaultValue(null)]
        public BasePart Base { get; set; }

        /// <inheritdoc/>
        UIElement IAssetPartDesign<UIElement>.Part { get { return UIElement; } set { UIElement = value; } }

        /// <inheritdoc />
        public bool Equals(UIElementDesign other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return UIElement.Equals(other.UIElement) && Equals(Base, other.Base);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((UIElementDesign)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode - this property is not supposed to be changed, except in initializers
            return UIElement.GetHashCode();
        }

        /// <inheritdoc />
        public static bool operator ==(UIElementDesign left, UIElementDesign right)
        {
            return Equals(left, right);
        }

        /// <inheritdoc />
        public static bool operator !=(UIElementDesign left, UIElementDesign right)
        {
            return !Equals(left, right);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"UIElementDesign [{UIElement.GetType().Name}, {UIElement.Name}]";
        }
    }
}
