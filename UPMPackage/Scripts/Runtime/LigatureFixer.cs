using System.Collections.Generic;

namespace RTLTMPro
{
    public static class LigatureFixer
    {
        private static readonly List<int> LtrTextHolder = new List<int>(512);
        private static readonly List<int> TagTextHolder = new List<int>(512);
        private static readonly Dictionary<char, char> MirroredCharsMap = new Dictionary<char, char>()
        {
            ['('] = ')',
            [')'] = '(',
            ['»'] = '«',
            ['«'] = '»'
        };
        private static readonly HashSet<char> MirroredCharsSet = new HashSet<char>(MirroredCharsMap.Keys);
        private static void FlushBufferToOutput(List<int> buffer, FastStringBuilder output)
        {
            for (int j = 0; j < buffer.Count; j++)
            {
                output.Append(buffer[buffer.Count - 1 - j]);
            }

            buffer.Clear();
        }

        /// <summary>
        ///     Fixes the flow of the text.
        /// </summary>
        public static void Fix(FastStringBuilder input, FastStringBuilder output, bool farsi, bool fixTextTags, bool preserveNumbers)
        {
            // Some texts like tags, English words and numbers need to be displayed in their original order.
            // This list keeps the characters that their order should be reserved and streams reserved texts into final letters.
            LtrTextHolder.Clear();
            TagTextHolder.Clear();
            for (int i = input.Length - 1; i >= 0; i--)
            {
                bool isInMiddle = i > 0 && i < input.Length - 1;
                bool isAtBeginning = i == 0;
                bool isAtEnd = i == input.Length - 1;

                int characterAtThisIndex = input.Get(i);

                int nextCharacter = default;
                if (!isAtEnd)
                    nextCharacter = input.Get(i + 1);

                int previousCharacter = default;
                if (!isAtBeginning)
                    previousCharacter = input.Get(i - 1);

                if (fixTextTags)
                {
                    if (characterAtThisIndex == '>')
                    {
                        // We need to check if it is actually the beginning of a tag.
                        bool isValidTag = false;
                        int nextI = i;
                        TagTextHolder.Add(characterAtThisIndex);

                        for (int j = i - 1; j >= 0; j--)
                        {
                            var jChar = input.Get(j);
                            
                            TagTextHolder.Add(jChar);

                            if (jChar == '<')
                            {
                                var jPlus1Char = input.Get(j + 1);
                                // Tags do not start with space
                                if (jPlus1Char == ' ')
                                {
                                    break;
                                }
                                isValidTag = true;
                                nextI = j;
                                break;
                            }
                        }

                        if (isValidTag)
                        {
                            FlushBufferToOutput(LtrTextHolder, output);
                            FlushBufferToOutput(TagTextHolder, output);
                            i = nextI;
                            continue;
                        } else
                        {
                            TagTextHolder.Clear();
                        }
                    }
                }

                if (Char32Utils.IsPunctuation(characterAtThisIndex) || Char32Utils.IsSymbol(characterAtThisIndex))
                {
                    if (MirroredCharsSet.Contains((char)characterAtThisIndex))
                    {
                        // IsRTLCharacter returns false for null
                        bool isAfterRTLCharacter = Char32Utils.IsRTLCharacter(previousCharacter);
                        bool isBeforeRTLCharacter = Char32Utils.IsRTLCharacter(nextCharacter);
                        bool isAfterQuote = Char32Utils.IsQuote(previousCharacter);                 // Corrects reversed brackets next to quote marks
                        bool isBeforeQuote = Char32Utils.IsQuote(nextCharacter);

                        if (isAfterRTLCharacter || isBeforeRTLCharacter || isAfterQuote || isBeforeQuote)
                        {
                            characterAtThisIndex = MirroredCharsMap[(char)characterAtThisIndex];
                        }
                    }

                    if (isInMiddle)
                    {
                        bool isAfterRTLCharacter = Char32Utils.IsRTLCharacter(previousCharacter);
                        bool isBeforeRTLCharacter = Char32Utils.IsRTLCharacter(nextCharacter);
                        bool isBeforeWhiteSpace = Char32Utils.IsWhiteSpace(nextCharacter);
                        bool isAfterWhiteSpace = Char32Utils.IsWhiteSpace(previousCharacter);
                        bool isAfterNumber = Char32Utils.IsNumber(previousCharacter, preserveNumbers, farsi);
                        bool isBeforeNumber = Char32Utils.IsNumber(nextCharacter, preserveNumbers, farsi);
                        bool isUnderline = characterAtThisIndex == '_';
                        bool isSpecialPunctuation = characterAtThisIndex is '.' or '،' or '؛' or '؟' or '=';
                        bool isBeforeSpecialPunctuation = nextCharacter is '.' or '،' or '؛' or '؟' or '=';
                        bool isAfterQuote = Char32Utils.IsQuote(previousCharacter);
                        bool isBeforeQuote = Char32Utils.IsQuote(nextCharacter);
                        bool isClosingBracket = characterAtThisIndex is '(';
                        bool isOpeningBracket = characterAtThisIndex is ')';
                        bool isBeforeBracket = nextCharacter is ')' or '(';
                        bool isAfterBracket = previousCharacter is ')' or '(';
                        bool isQuote = Char32Utils.IsQuote(characterAtThisIndex);
                        bool isAfterLetter = Char32Utils.IsLetter(previousCharacter);
                        bool isBeforeLetter = Char32Utils.IsLetter(nextCharacter);


                        if (isBeforeRTLCharacter && isAfterRTLCharacter ||
                            isAfterWhiteSpace && isSpecialPunctuation ||
                            isBeforeWhiteSpace && isAfterRTLCharacter ||
                            isBeforeRTLCharacter && isAfterWhiteSpace ||
                            isBeforeWhiteSpace && isAfterNumber && isSpecialPunctuation ||
                            isClosingBracket && isBeforeQuote ||                            // Corrects "( in "(المريخ)"
                            isOpeningBracket && isAfterQuote ||                             // Corrects )" in "(المريخ)"
                            isQuote && isAfterBracket ||                                    // Corrects ") in .("المريخ") and :"( in :"(المريخ)":
                            isQuote && isBeforeBracket ||                                   // Corrects (" in ("المريخ") and )": in :"(المريخ)":
                            isClosingBracket && isBeforeSpecialPunctuation ||               // Corrects .( in .(المريخ)
                            isQuote && (isAfterLetter || isAfterNumber) ||                  // Corrects "aaa in "aaaموسيقى" 
                            isQuote && (isBeforeLetter || isBeforeNumber) ||                // Corrects aaa" in "موسيقىaaa"
                            isSpecialPunctuation && isAfterLetter ||                        // Corrects .E in .ENGAGE LINK
                            (isBeforeRTLCharacter || isAfterRTLCharacter) && isUnderline) 
                        {
                            FlushBufferToOutput(LtrTextHolder, output);
                            output.Append(characterAtThisIndex);
                        } else
                        {
                            LtrTextHolder.Add(characterAtThisIndex);
                        }
                    } else if (isAtEnd)
                    {
                        // Check if the punctuation comes at the end of the string and follows a number
                        bool isAfterNumber = Char32Utils.IsNumber(previousCharacter, preserveNumbers, farsi);
                        bool isSpecialPunctuation = characterAtThisIndex is '.' or '،' or '؛' or '؟';

                        // Check if the punctuation comes at the end of the string and follows a quotation mark
                        bool isAfterQuote = Char32Utils.IsQuote(previousCharacter);

                        if (isSpecialPunctuation && (isAfterNumber || isAfterQuote))
                        {
                            FlushBufferToOutput(LtrTextHolder, output);
                            output.Append(characterAtThisIndex);
                        }
                        else
                        {
                            LtrTextHolder.Add(characterAtThisIndex);
                        }

                    } else if (isAtBeginning)
                    {
                        output.Append(characterAtThisIndex);
                    }

                    continue;
                }

                if (isInMiddle)
                {
                    bool isAfterEnglishChar = Char32Utils.IsEnglishLetter(previousCharacter);
                    bool isBeforeEnglishChar = Char32Utils.IsEnglishLetter(nextCharacter);
                    bool isAfterNumber = Char32Utils.IsNumber(previousCharacter, preserveNumbers, farsi);
                    bool isBeforeNumber = Char32Utils.IsNumber(nextCharacter, preserveNumbers, farsi);
                    bool isAfterSymbol = Char32Utils.IsSymbol(previousCharacter);
                    bool isBeforeSymbol = Char32Utils.IsSymbol(nextCharacter);

                    // For cases where english words and farsi/arabic are mixed. This allows for using farsi/arabic, english and numbers in one sentence.
                    // If the space is between numbers,symbols or English words, keep the order
                    if (characterAtThisIndex == ' ' &&
                        (isBeforeEnglishChar || isBeforeNumber || isBeforeSymbol) &&
                        (isAfterEnglishChar || isAfterNumber || isAfterSymbol))
                    {
                        LtrTextHolder.Add(characterAtThisIndex);
                        continue;
                    }
                }

                if (Char32Utils.IsEnglishLetter(characterAtThisIndex) ||
                    Char32Utils.IsNumber(characterAtThisIndex, preserveNumbers, farsi))
                {
                    LtrTextHolder.Add(characterAtThisIndex);
                    continue;
                }

                // handle surrogates
                if (characterAtThisIndex >= (char)0xD800 &&
                    characterAtThisIndex <= (char)0xDBFF ||
                    characterAtThisIndex >= (char)0xDC00 && characterAtThisIndex <= (char)0xDFFF)
                {
                    LtrTextHolder.Add(characterAtThisIndex);
                    continue;
                }

                FlushBufferToOutput(LtrTextHolder, output);

                if (characterAtThisIndex != 0xFFFF &&
                    characterAtThisIndex != (int)SpecialCharacters.ZeroWidthNoJoiner)
                {
                    output.Append(characterAtThisIndex);
                }
            }

            FlushBufferToOutput(LtrTextHolder, output);
        }
    }
}