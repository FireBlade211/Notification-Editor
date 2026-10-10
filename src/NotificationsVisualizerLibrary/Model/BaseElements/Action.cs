using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using System.Xml.Linq;
using NotificationsVisualizerLibrary.Parsers;
using NotificationsVisualizerLibrary.Model.Enums;

namespace NotificationsVisualizerLibrary.Model.BaseElements
{
    [ObjectModelClass("ToastButton", NotificationType.Toast)]
    internal class Action : AdaptiveChildElement, IActivatableElement
    {
        public Action(NotificationType context, FeatureSet supportedFeatures) : base(context, supportedFeatures) { }

        internal const string ATTR_CONTENT = "content";
        internal const string ATTR_IMAGEURI = "imageUri";
        internal const string ATTR_HINT_INPUTID = "hint-inputId";
        internal const string ATTR_PLACEMENT = "placement";
        internal const string ATTR_HINT_TOOLTIP = "hint-toolTip";
        internal const string ATTR_HINT_BUTTONSTYLE = "hint-buttonStyle";

        public string Id { get; set; }

        public ActionPlacement Placement { get; set; }

        public string Content { get; set; }

        public string Arguments { get; set; }

        [ObjectModelProperty("ActivationType")]
        public ActivationType ActivationType { get; set; } = ActivationType.Foreground;

        [ObjectModelProperty("ImageUri")]
        public string ImageUri { get; set; }

        [ObjectModelProperty("InputId")]
        public string InputId { get; set; }

        [ObjectModelProperty("ToolTip")]
        public string ToolTip { get; set; }

        public bool HintVisible { get; set; } = true;
        public string Type { get; set; }
        public AfterActivationBehavior AfterActivationBehavior { get; set; }
        public ToastButtonStyle HintButtonStyle { get; set; }

        protected override IEnumerable<string> GetAttributesNotSupportedByVisualizer()
        {
            return new string[] { };
        }

        internal void Parse(ParseResult result, XElement node)
        {
            AttributesHelper attributes = new AttributesHelper(node.Attributes());

            ParseKnownAttributes(node, attributes, result);

            HandleRemainingAttributes(attributes, result);
        }

        internal virtual void ParseKnownAttributes(XElement node, AttributesHelper attributes, ParseResult result)
        {
            // content is required
            XAttribute attrContent = attributes.PopAttribute(ATTR_CONTENT);
            if (attrContent == null)
            {
                result.AddError("content attribute on action element is required.", XmlTemplateParser.GetErrorPositionInfo(node));
                throw new IncompleteElementException();
            }

            this.Content = attrContent.Value;

            // parse activationType, arguments, state
            base.ParseActivatableElementAttributes(node, attributes, result);


            // imageUri is optional
            var attrImageUri = attributes.PopAttribute(ATTR_IMAGEURI);
            if (attrImageUri != null)
                this.ImageUri = attrImageUri.Value;

            // inputId is optional
            var attrInputId = attributes.PopAttribute(ATTR_HINT_INPUTID);
            if (attrInputId != null)
                this.InputId = attrInputId.Value;

            // toolTip is optional
            var attrToolTip = attributes.PopAttribute(ATTR_HINT_TOOLTIP);
            if (attrToolTip != null)
                this.ToolTip = attrToolTip.Value;

            // placement is optional
            ActionPlacement placement;
            if (TryParseEnum(result, attributes, ATTR_PLACEMENT, out placement))
                this.Placement = placement;

            ToastButtonStyle style;
            if (TryParseEnum(result, attributes, ATTR_HINT_BUTTONSTYLE, out style))
                this.HintButtonStyle = style;
        }

        protected override Array GetSupportedEnums<TEnum>()
        {
            // Override the allowed text placement values, since they depend on OS version / supported features
            if (typeof(TEnum) == typeof(ActionPlacement))
                switch (Context)
                {
                    case NotificationType.Toast:

                        if (SupportedFeatures.ToastContextMenu)
                            return new ActionPlacement[] { ActionPlacement.Inline, ActionPlacement.ContextMenu };

                        return new ActionPlacement[] { ActionPlacement.Inline };



                    default:
                        return new ActionPlacement[] { ActionPlacement.Inline };
                }

            return base.GetSupportedEnums<TEnum>();
        }

        public override ObjectModelObject ConvertToObject()
        {
            var obj = base.ConvertToObject();

            obj.ConstructorValues.Add(new ObjectModelString(Content));
            obj.ConstructorValues.Add(new ObjectModelString(Arguments));

            switch (Placement)
            {
                case ActionPlacement.ContextMenu:
                    obj.Name = "ToastContextMenuItem";
                    break;

                default:
                    switch (ActivationType)
                    {
                        case ActivationType.System:
                            obj.PropertyValues.Clear();
                            var args = Arguments != null ? Arguments : "";
                            switch (args.ToLower())
                            {
                                case "snooze":
                                    obj = new ObjectModelObject("ToastButtonSnooze");
                                    if (!string.IsNullOrWhiteSpace(Content))
                                    {
                                        obj.ConstructorValues.Add(new ObjectModelString(Content));
                                    }
                                    if (InputId != null)
                                    {
                                        obj.Add("SelectionBoxId", new ObjectModelString(InputId));
                                    }
                                    break;

                                default:
                                    obj = new ObjectModelObject("ToastButtonDismiss");
                                    if (!string.IsNullOrWhiteSpace(Content))
                                    {
                                        obj.ConstructorValues.Add(new ObjectModelString(Content));
                                    }
                                    break;
                            }
                            break;
                    }
                    break;
            }

            return obj;
        }
    }

    public enum ToastButtonStyle
    {
        Default,
        Success,
        Critical
    }
}
