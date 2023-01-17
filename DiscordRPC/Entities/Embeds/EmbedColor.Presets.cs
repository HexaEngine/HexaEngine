// MIT License
//
// Copyright(c) 2022 Bujju
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

namespace DiscordIntegration.Entities.Embeds
{
    public partial struct EmbedColor
    {
        /// <summary>
        ///     Gets an <see cref="EmbedColor"/> representing black.
        /// </summary>
        public static EmbedColor Black { get; } = new EmbedColor("000000");

        /// <summary>
        ///     Gets an <see cref="EmbedColor"/> representing gray.
        /// </summary>
        public static EmbedColor Gray { get; } = new EmbedColor("808080");

        /// <summary>
        ///     Gets an <see cref="EmbedColor"/> representing white.
        /// </summary>
        public static EmbedColor White { get; } = new EmbedColor("FEFEFE");

        /// <summary>
        ///     Gets an <see cref="EmbedColor"/> representing red.
        /// </summary>
        public static EmbedColor Red { get; } = new EmbedColor("FF0000");

        /// <summary>
        ///     Gets an <see cref="EmbedColor"/> representing pink.
        /// </summary>
        public static EmbedColor Pink { get; } = new EmbedColor("FF40BF");

        /// <summary>
        ///     Gets an <see cref="EmbedColor"/> representing burgundy.
        /// </summary>
        public static EmbedColor Burgundy { get; } = new EmbedColor("800040");

        /// <summary>
        ///     Gets an <see cref="EmbedColor"/> representing orange.
        /// </summary>
        public static EmbedColor Orange { get; } = new EmbedColor("FF8000");

        /// <summary>
        ///     Gets an <see cref="EmbedColor"/> representing brown.
        /// </summary>
        public static EmbedColor Brown { get; } = new EmbedColor("804000");

        /// <summary>
        ///     Gets an <see cref="EmbedColor"/> representing yellow.
        /// </summary>
        public static EmbedColor Yellow { get; } = new EmbedColor("FFFF00");

        /// <summary>
        ///     Gets an <see cref="EmbedColor"/> representing green.
        /// </summary>
        public static EmbedColor Green { get; } = new EmbedColor("104000");

        /// <summary>
        ///     Gets an <see cref="EmbedColor"/> representing lime green.
        /// </summary>
        public static EmbedColor LimeGreen { get; } = new EmbedColor("00FF00");

        /// <summary>
        ///     Gets an <see cref="EmbedColor"/> representing emerald.
        /// </summary>
        public static EmbedColor Emerald { get; } = new EmbedColor("106440");

        /// <summary>
        ///     Gets an <see cref="EmbedColor"/> representing olive.
        /// </summary>
        public static EmbedColor Olive { get; } = new EmbedColor("648040");

        /// <summary>
        ///     Gets an <see cref="EmbedColor"/> representing blue.
        /// </summary>
        public static EmbedColor Blue { get; } = new EmbedColor("0000FF");

        /// <summary>
        ///     Gets an <see cref="EmbedColor"/> representing light blue.
        /// </summary>
        public static EmbedColor LightBlue { get; } = new EmbedColor("00E1FF");

        /// <summary>
        ///     Gets an <see cref="EmbedColor"/> representing turquoise.
        /// </summary>
        public static EmbedColor Turquoise { get; } = new EmbedColor("00FFC8");

        /// <summary>
        ///     Gets an <see cref="EmbedColor"/> representing purple.
        /// </summary>
        public static EmbedColor Purple { get; } = new EmbedColor("8000FF");

        /// <summary>
        ///     Gets an <see cref="EmbedColor"/> representing lilac.
        /// </summary>
        public static EmbedColor Lilac { get; } = new EmbedColor("BD8BC7");

        /// <summary>
        ///     Gets an <see cref="EmbedColor"/> representing periwinkle.
        /// </summary>
        public static EmbedColor Periwinkle { get; } = new EmbedColor("A582FF");

        /// <summary>
        ///     Gets an <see cref="EmbedColor"/> representing magenta.
        /// </summary>
        public static EmbedColor Magenta { get; } = new EmbedColor("FF00FF");

        /// <summary>
        ///     Gets an <see cref="EmbedColor"/> representing Discord's branding color.
        /// </summary>
        public static EmbedColor Blurple { get; } = new EmbedColor("6064F4");

        /// <summary>
        ///     Gets an <see cref="EmbedColor"/> that blends in with the rest of the embed in dark mode.
        /// </summary>
        public static EmbedColor InvisibleDark { get; } = new EmbedColor("303434");

        /// <summary>
        ///     Gets an <see cref="EmbedColor"/> that blends in with the rest of the embed in light mode.
        /// </summary>
        public static EmbedColor InvisibleLight { get; } = new EmbedColor("F8F4F4");
    }
}