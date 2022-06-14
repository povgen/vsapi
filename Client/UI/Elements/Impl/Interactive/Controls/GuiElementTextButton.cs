﻿using Cairo;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace Vintagestory.API.Client
{
    public enum EnumButtonStyle
    {
        None,
        MainMenu,
        Normal,
        Small
    }

    public class GuiElementTextButton : GuiElementControl
    {
        GuiElementStaticText normalText;
        GuiElementStaticText pressedText;

        LoadedTexture normalTexture;
        LoadedTexture hoverTexture;
        LoadedTexture disabledTexture;
        
        ActionConsumable onClick;

        bool isOver;

        EnumButtonStyle buttonStyle;
        bool active = false;
        bool currentlyMouseDownOnElement = false;

        public bool PlaySound = true;

        public static double Padding = 2;

        double textOffsetY;

        public bool Visible = true;

        public override bool Focusable { get { return true; } }

        /// <summary>
        /// Creates a button with text.
        /// </summary>
        /// <param name="capi">The Client API</param>
        /// <param name="text">The text of the button.</param>
        /// <param name="font">The font of the text.</param>
        /// <param name="hoverFont">The font of the text when the player is hovering over the button.</param>
        /// <param name="onClick">The event fired when the button is clicked.</param>
        /// <param name="bounds">The bounds of the button.</param>
        /// <param name="style">The style of the button.</param>
        public GuiElementTextButton(ICoreClientAPI capi, string text, CairoFont font, CairoFont hoverFont, ActionConsumable onClick, ElementBounds bounds, EnumButtonStyle style = EnumButtonStyle.Normal) : base(capi, bounds)
        {
            hoverTexture = new LoadedTexture(capi);
            normalTexture = new LoadedTexture(capi);
            disabledTexture = new LoadedTexture(capi);
            this.buttonStyle = style;

            normalText = new GuiElementStaticText(capi, text, EnumTextOrientation.Center, bounds.CopyOnlySize(), font);
            normalText.AutoBoxSize(true);

            pressedText = new GuiElementStaticText(capi, text, EnumTextOrientation.Center, bounds.CopyOnlySize(), hoverFont);

            this.onClick = onClick;
        }

        /// <summary>
        /// Sets the orientation of the text both when clicked and when idle.
        /// </summary>
        /// <param name="orientation">The orientation of the text.</param>
        public void SetOrientation(EnumTextOrientation orientation)
        {
            normalText.orientation = orientation;
            pressedText.orientation = orientation;
        }

        public override void BeforeCalcBounds()
        {
            normalText.AutoBoxSize(true);
            Bounds.fixedWidth = normalText.Bounds.fixedWidth;
            Bounds.fixedHeight = normalText.Bounds.fixedHeight;

            pressedText.Bounds = normalText.Bounds.CopyOnlySize();
        }

        public override void ComposeElements(Context ctxStatic, ImageSurface surfaceStatic)
        {
            Bounds.CalcWorldBounds();
            normalText.Bounds.CalcWorldBounds();

            var surface = new ImageSurface(Format.Argb32, (int)Bounds.OuterWidth, (int)Bounds.OuterHeight);
            var ctx = genContext(surface);

            // 1. Normal button
            ComposeButton(ctx, surface);
            generateTexture(surface, ref normalTexture);
            ctx.Clear();


            // 2. Hover button
            if (buttonStyle != EnumButtonStyle.None)
            {
                ctx.SetSourceRGBA(0, 0, 0, 0.4);
                ctx.Rectangle(0, 0, Bounds.OuterWidth, Bounds.OuterHeight);
                ctx.Fill();
            }

            pressedText.Bounds.fixedY += textOffsetY;
            pressedText.ComposeElements(ctx, surface);
            pressedText.Bounds.fixedY -= textOffsetY;

            generateTexture(surface, ref hoverTexture);

            ctx.Dispose();
            surface.Dispose();


            // 3. Disabled button
            surface = new ImageSurface(Format.Argb32, 2, 2);
            ctx = genContext(surface);

            if (buttonStyle != EnumButtonStyle.None)
            {
                ctx.SetSourceRGBA(0, 0, 0, 0.4);
                ctx.Rectangle(0, 0, 2, 2);
                ctx.Fill();
            }

            generateTexture(surface, ref disabledTexture);

            ctx.Dispose();
            surface.Dispose();
        }


        void ComposeButton(Context ctx, ImageSurface surface)
        {
            double embossHeight = scaled(2.5);

            if (buttonStyle == EnumButtonStyle.Normal || buttonStyle == EnumButtonStyle.Small)
            {
                embossHeight = scaled(1.5);
            }


            if (buttonStyle != EnumButtonStyle.None)
            {
                // Brown background
                Rectangle(ctx, 0, 0, Bounds.OuterWidth, Bounds.OuterHeight);
                ctx.SetSourceRGBA(69 / 255.0, 52 / 255.0, 36 / 255.0, 1);
                ctx.Fill();
            }

            if (buttonStyle == EnumButtonStyle.MainMenu)
            {
                // Top shine
                Rectangle(ctx, 0, 0, Bounds.OuterWidth, embossHeight);
                ctx.SetSourceRGBA(1, 1, 1, 0.15);
                ctx.Fill();
            }

            if (buttonStyle == EnumButtonStyle.Normal || buttonStyle == EnumButtonStyle.Small)
            {
                // Top shine
                Rectangle(ctx, 0, 0, Bounds.OuterWidth - embossHeight, embossHeight);
                ctx.SetSourceRGBA(1, 1, 1, 0.15);
                ctx.Fill();

                // Left shine
                Rectangle(ctx, 0, 0 + embossHeight, embossHeight, Bounds.OuterHeight - embossHeight);
                ctx.SetSourceRGBA(1, 1, 1, 0.15);
                ctx.Fill();
            }

            surface.BlurPartial(2, 5);

            // Pretty elaborate way of vertically centering the text. Le sigh.
            FontExtents fontex = normalText.Font.GetFontExtents();
            TextExtents textex = normalText.Font.GetTextExtents(normalText.GetText());
            double resetY = -fontex.Ascent - textex.YBearing;
            textOffsetY = (resetY + (normalText.Bounds.InnerHeight + textex.YBearing) / 2) / RuntimeEnv.GUIScale;

            normalText.Bounds.fixedY += textOffsetY;
            normalText.ComposeElements(ctx, surface);
            normalText.Bounds.fixedY -= textOffsetY;

            // ShowMultilineText changes height
            Bounds.CalcWorldBounds();

            if (buttonStyle == EnumButtonStyle.MainMenu)
            {
                // Bottom shade
                Rectangle(ctx, 0, 0 + Bounds.OuterHeight - embossHeight, Bounds.OuterWidth, embossHeight);
                ctx.SetSourceRGBA(0, 0, 0, 0.2);
                ctx.Fill();
            }

            if (buttonStyle == EnumButtonStyle.Normal || buttonStyle == EnumButtonStyle.Small)
            {
                // Bottom shade
                Rectangle(ctx, 0 + embossHeight, 0 + Bounds.OuterHeight - embossHeight, Bounds.OuterWidth - 2*embossHeight, embossHeight);
                ctx.SetSourceRGBA(0, 0, 0, 0.2);
                ctx.Fill();

                // Right shade
                Rectangle(ctx, 0 + Bounds.OuterWidth - embossHeight, 0, embossHeight, Bounds.OuterHeight);
                ctx.SetSourceRGBA(0, 0, 0, 0.2);
                ctx.Fill();
            }
        }


        public override void RenderInteractiveElements(float deltaTime)
        {
            if (!Visible) return;

            api.Render.Render2DTexturePremultipliedAlpha(normalTexture.TextureId, Bounds);

            if (!enabled)
            {
                api.Render.Render2DTexturePremultipliedAlpha(disabledTexture.TextureId, Bounds);
            }
            else if (isOver || currentlyMouseDownOnElement)
            {
                api.Render.Render2DTexturePremultipliedAlpha(hoverTexture.TextureId, Bounds);
            }
        }



        public override void OnKeyDown(ICoreClientAPI api, KeyEvent args)
        {
            if (!Visible) return;
            if (!HasFocus) return;
            if (args.KeyCode == (int)GlKeys.Enter)
            {
                args.Handled = true;
                if (enabled)
                {
                    if (PlaySound)
                    {
                        api.Gui.PlaySound("menubutton_press");
                    }
                    args.Handled = onClick();
                }
            }
        }


        public override void OnMouseMove(ICoreClientAPI api, MouseEvent args)
        {
            if (!Visible) return;
            if ((enabled && Bounds.PointInside(api.Input.MouseX, api.Input.MouseY)) || active)
            {
                if (!isOver && PlaySound) api.Gui.PlaySound("menubutton");
                isOver = true;

            }
            else
            {
                isOver = false;
            }
        }

        public override void OnMouseDownOnElement(ICoreClientAPI api, MouseEvent args)
        {
            if (!Visible) return;
            if (!enabled) return;

            base.OnMouseDownOnElement(api, args);

            currentlyMouseDownOnElement = true;
        }

        public override void OnMouseUp(ICoreClientAPI api, MouseEvent args)
        {
            if (!Visible) return;

            base.OnMouseUp(api, args);

            currentlyMouseDownOnElement = false;
        }


        public override void OnMouseUpOnElement(ICoreClientAPI api, MouseEvent args)
        {
            if (enabled && currentlyMouseDownOnElement && Bounds.PointInside(args.X, args.Y) && args.Button == EnumMouseButton.Left)
            {
                if (PlaySound)
                {
                    api.Gui.PlaySound("menubutton_press");
                }
                args.Handled = onClick();
            }

            currentlyMouseDownOnElement = false;
        }



        /// <summary>
        /// Sets the button as active or inactive.
        /// </summary>
        /// <param name="active">Active == clickable</param>
        public void SetActive(bool active)
        {
            this.active = active;
        }

        public override void Dispose()
        {
            base.Dispose();

            hoverTexture.Dispose();
            normalText?.Dispose();
            pressedText?.Dispose();
            disabledTexture?.Dispose();
            normalTexture?.Dispose();
        }
    }


    public static partial class GuiComposerHelpers
    {
        /// <summary>
        /// Creates a button for the current GUI.
        /// </summary>
        /// <param name="text">The text on the button.</param>
        /// <param name="onClick">The event fired when the button is clicked.</param>
        /// <param name="bounds">The bounds of the button.</param>
        /// <param name="buttonFont">The font of the button.</param>
        /// <param name="style">The style of the button. (Default: Normal)</param>
        /// <param name="orientation">The orientation of the text. (Default: center)</param>
        /// <param name="key">The internal name of the button.</param>
        public static GuiComposer AddButton(this GuiComposer composer, string text, ActionConsumable onClick, ElementBounds bounds, CairoFont buttonFont, EnumButtonStyle style = EnumButtonStyle.Normal, EnumTextOrientation orientation = EnumTextOrientation.Center, string key = null)
        {
            if (!composer.Composed)
            {
                CairoFont hoverFont = buttonFont.Clone().WithColor(GuiStyle.ActiveButtonTextColor);
                GuiElementTextButton elem = new GuiElementTextButton(composer.Api, text, buttonFont, hoverFont, onClick, bounds, style);
                elem.SetOrientation(orientation);
                composer.AddInteractiveElement(elem, key);

            }
            return composer;
        }

        /// <summary>
        /// Creates a button for the current GUI.
        /// </summary>
        /// <param name="text">The text on the button.</param>
        /// <param name="onClick">The event fired when the button is clicked.</param>
        /// <param name="bounds">The bounds of the button.</param>
        /// <param name="style">The style of the button. (Default: Normal)</param>
        /// <param name="orientation">The orientation of the text. (Default: center)</param>
        /// <param name="key">The internal name of the button.</param>
        public static GuiComposer AddButton(this GuiComposer composer, string text, ActionConsumable onClick, ElementBounds bounds, EnumButtonStyle style = EnumButtonStyle.Normal, EnumTextOrientation orientation = EnumTextOrientation.Center, string key = null)
        {
            if (!composer.Composed)
            {
                GuiElementTextButton elem = new GuiElementTextButton(composer.Api, text, CairoFont.ButtonText(), CairoFont.ButtonPressedText(), onClick, bounds, style);
                elem.SetOrientation(orientation);
                composer.AddInteractiveElement(elem, key);
                
            }
            return composer;
        }

        /// <summary>
        /// Creates a small button for the current GUI.
        /// </summary>
        /// <param name="text">The text on the button.</param>
        /// <param name="onClick">The event fired when the button is clicked.</param>
        /// <param name="bounds">The bounds of the button.</param>
        /// <param name="style">The style of the button. (Default: Normal)</param>
        /// <param name="orientation">The orientation of the text. (Default: center)</param>
        /// <param name="key">The internal name of the button.</param>
        public static GuiComposer AddSmallButton(this GuiComposer composer, string text, ActionConsumable onClick, ElementBounds bounds, EnumButtonStyle style = EnumButtonStyle.Normal, EnumTextOrientation orientation = EnumTextOrientation.Center, string key = null)
        {
            if (!composer.Composed)
            {
                CairoFont font1 = CairoFont.ButtonText();
                CairoFont font2 = CairoFont.ButtonPressedText();
                font1.Fontname = GuiStyle.StandardFontName;
                font2.Fontname = GuiStyle.StandardFontName;
                if (style != EnumButtonStyle.Small)
                {
                    font1.FontWeight = FontWeight.Bold;
                    font2.FontWeight = FontWeight.Bold;
                } else
                {
                    font1.FontWeight = FontWeight.Normal;
                    font2.FontWeight = FontWeight.Normal;
                }
                font1.UnscaledFontsize = GuiStyle.SmallFontSize;
                font2.UnscaledFontsize = GuiStyle.SmallFontSize;

                GuiElementTextButton elem = new GuiElementTextButton(composer.Api, text, font1, font2, onClick, bounds, style);
                elem.SetOrientation(orientation);
                composer.AddInteractiveElement(elem, key);
            }
            return composer;
        }

        /// <summary>
        /// Gets the button by name.
        /// </summary>
        /// <param name="key">The name of the button.</param>
        public static GuiElementTextButton GetButton(this GuiComposer composer, string key)
        {
            return (GuiElementTextButton)composer.GetElement(key);
        }
       

    }

}
