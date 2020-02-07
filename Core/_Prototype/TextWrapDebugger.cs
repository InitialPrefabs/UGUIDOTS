using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UGUIDots {

    // TODO: This does not help in debugging \n characters cause I don't need it currently
    public class TextWrapDebugger : MonoBehaviour {

        // I think at the very least - all I need is the width of the line so I can compute the center of the text
        struct LineDiagnostics {
            public int WordCount;           // How many words are in the text?
            public int LineCharCount;       // How many chars in the line? I think this would be helpful for error checking 
            public int WordCharCount;       // How many chars are in the current word - this is great for spans. Although idk how I'm going to use this yet...

            // I think this is helpful so in the current example, has is the start of a new word on a new line - 
            // So we probably want to subtract width of the word from the line width so that it does not include
            // the h
            public float WordWidth;         // How wide is the word? Can be used to figure out the actual length of the line.
            public float LineWidth;         // How wide is the line, non adjusted so this is the max # of widths you can stuff into a lien?

            // NOTE: I don't think the span is highly important cause it's somewhat difficult to compute how many chars
            // are in the line

            public override string ToString() {
                return $"LineWidth: {LineWidth}, Latest Word Width: {WordWidth}, LineWordCount: {LineCharCount}";
            }
        }

        List<LineDiagnostics> lineInfos;

        void OnDrawGizmos() {
            if (lineInfos == null) {
                lineInfos = new List<LineDiagnostics>();
            }
            lineInfos.Clear();

            var text     = GetComponent<TextMeshProUGUI>();
            var rectSize = text.rectTransform.rect.size;

            var fontScale   = text.fontSize / (float)text.font.faceInfo.pointSize;
            var glyphs      = text.font.characterLookupTable;
            var diagnostics = new LineDiagnostics {};

            foreach (var c in text.text) {
                glyphs.TryGetValue((uint)c, out var ch);

                var advance = ch.glyph.metrics.horizontalAdvance * fontScale;

                // Foreach space increment the word count.
                if (c == ' ') {
                    diagnostics.WordCount++;

                    // Reset the word width since there will be a new word, also make the count of the word -1
                    diagnostics.WordWidth     = 0f;

                    // Reset the word count so that a space incremented would just be 0
                    diagnostics.WordCharCount = -1;
                }

                // Increment the # of chars per word
                diagnostics.WordCharCount++;
                diagnostics.LineCharCount++;
                diagnostics.LineWidth += advance;
                diagnostics.WordWidth += advance;

                if (diagnostics.LineWidth > rectSize.x) {
                    // Recompute the line's width if we scanned any new letters that are part of a new word
                    diagnostics.LineWidth     -= diagnostics.WordWidth;
                    diagnostics.LineCharCount -= diagnostics.WordCharCount;

                    lineInfos.Add(diagnostics);

                    Debug.Log($"<color=red>Next line width: {diagnostics.WordWidth}</color>");

                    diagnostics.LineWidth     = diagnostics.WordWidth;
                    diagnostics.WordWidth     = 0f;
                    diagnostics.LineCharCount = diagnostics.WordCharCount;

                    Debug.Log($"<color=green>New Line With Char: {c}</color>");
                }

            }
            lineInfos.Add(diagnostics);

            Debug.Log($"# of lines: {lineInfos.Count}");

            foreach (var info in lineInfos) {
                Debug.Log(info.ToString());
            }
        }
    }
}
