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

using DiscordRPC.Entities.Embeds;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DiscordIntegration.Entities.Embeds
{
    /// <summary>
    ///     Color information for <see cref="Embed"/> objects.
    /// </summary>
    public partial struct EmbedColor
    {
        internal int _value;

        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="EmbedColor"/> struct.
        /// </summary>
        /// <param name="hexColor">Hex color code for the color.</param>
        public EmbedColor(string hexColor)
            => Hexadecimal = hexColor;

        /// <summary>
        ///     Initializes a new instance of the <see cref="EmbedColor"/> struct.
        /// </summary>
        /// <param name="decimalColor">Decimal color code for the color.</param>
        public EmbedColor(decimal decimalColor)
            => Decimal = decimalColor;

        /// <summary>
        ///     Initializes a new instance of the <see cref="EmbedColor"/> struct.
        /// </summary>
        /// <param name="r">Red color value.</param>
        /// <param name="g">Green color value.</param>
        /// <param name="b">Blue color value.</param>
        public EmbedColor(short r, short g, short b)
            => RGB = (r, g, b);
        #endregion

        #region Properties
        /// <summary>
        ///     Gets or sets the hex color code for the color.
        /// </summary>
        public string Hexadecimal
        {
            get => _value.ToString("X");
            set
            {
                if (!Regex.IsMatch(value, "(?:[0-9a-fA-F]{3}){1,2}$"))
                    throw new FormatException("Invalid hexadecimal color code.");

                _value = int.Parse(value, NumberStyles.HexNumber);
            }
        }

        /// <summary>
        ///     Gets or sets the decimal color code for the color.
        /// </summary>
        public decimal Decimal
        {
            get => _value;
            set => _value = (int)value;
        }

        /// <summary>
        ///     Gets or sets the RGB color code for the color.
        /// </summary>
        public (short R, short G, short B) RGB
        {
            get => ((short)(_value >> 16), (short)(_value >> 8), (short)_value);
            set => _value = (value.R << 16) + (value.G << 8) + value.B;
        }
        #endregion
    }
}