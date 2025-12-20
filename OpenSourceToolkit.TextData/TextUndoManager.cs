using System;
using System.Collections.Generic;

namespace OpenSourceToolkit.TextData
{
    /// <summary>
    /// Manages undo/redo history for text content with time-based grouping.
    /// Consecutive edits within the grouping interval are merged into a single undo state.
    /// </summary>
    public class TextUndoManager
    {
        private readonly List<UndoState> _undoStack = new List<UndoState>();
        private readonly List<UndoState> _redoStack = new List<UndoState>();
        private DateTime _lastEditTime = DateTime.MinValue;
        private readonly TimeSpan _groupingInterval;

        /// <summary>
        /// Creates a new TextUndoManager with the specified grouping interval.
        /// </summary>
        /// <param name="groupingIntervalMs">Edits within this interval (ms) are grouped. Default 800ms like VS.</param>
        public TextUndoManager(int groupingIntervalMs = 800)
        {
            _groupingInterval = TimeSpan.FromMilliseconds(groupingIntervalMs);
        }

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;
        public int UndoCount => _undoStack.Count;
        public int RedoCount => _redoStack.Count;

        public event EventHandler StateChanged;

        /// <summary>
        /// Records a text change for undo. Call this BEFORE applying the change.
        /// </summary>
        /// <param name="currentText">The current text before the change.</param>
        /// <param name="forceNewState">If true, always creates a new undo state (for programmatic changes).</param>
        public void RecordChange(string currentText, bool forceNewState = false)
        {
            var now = DateTime.Now;
            var timeSinceLastEdit = now - _lastEditTime;

            // If within grouping interval and not forced, update the last state instead of creating new
            if (!forceNewState && _undoStack.Count > 0 && timeSinceLastEdit < _groupingInterval)
            {
                // Don't update the stored text - we want to undo to the state BEFORE the grouped edits
                _lastEditTime = now;
                return;
            }

            // Create new undo state
            _undoStack.Add(new UndoState
            {
                Text = currentText,
                Timestamp = now
            });

            // Clear redo stack when new changes are made
            _redoStack.Clear();
            _lastEditTime = now;

            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Performs undo operation.
        /// </summary>
        /// <param name="currentText">Current text to save for redo.</param>
        /// <returns>The text to restore, or null if nothing to undo.</returns>
        public string Undo(string currentText)
        {
            if (!CanUndo) return null;

            // Save current state for redo
            _redoStack.Add(new UndoState
            {
                Text = currentText,
                Timestamp = DateTime.Now
            });

            // Pop and return the last undo state
            var state = _undoStack[_undoStack.Count - 1];
            _undoStack.RemoveAt(_undoStack.Count - 1);

            // Reset grouping timer to prevent accidental grouping after undo
            _lastEditTime = DateTime.MinValue;

            StateChanged?.Invoke(this, EventArgs.Empty);
            return state.Text;
        }

        /// <summary>
        /// Performs redo operation.
        /// </summary>
        /// <param name="currentText">Current text to save for undo.</param>
        /// <returns>The text to restore, or null if nothing to redo.</returns>
        public string Redo(string currentText)
        {
            if (!CanRedo) return null;

            // Save current state for undo
            _undoStack.Add(new UndoState
            {
                Text = currentText,
                Timestamp = DateTime.Now
            });

            // Pop and return the last redo state
            var state = _redoStack[_redoStack.Count - 1];
            _redoStack.RemoveAt(_redoStack.Count - 1);

            // Reset grouping timer
            _lastEditTime = DateTime.MinValue;

            StateChanged?.Invoke(this, EventArgs.Empty);
            return state.Text;
        }

        /// <summary>
        /// Clears all undo/redo history.
        /// </summary>
        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
            _lastEditTime = DateTime.MinValue;
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        private class UndoState
        {
            public string Text { get; set; }
            public DateTime Timestamp { get; set; }
        }
    }
}
