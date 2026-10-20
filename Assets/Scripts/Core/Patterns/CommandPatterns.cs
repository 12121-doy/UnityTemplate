using System;
using System.Collections.Generic;
namespace Guisha.Core.Patterns
{
    public interface ICommand
    {
        bool CanExecute { get; }
        void Execute();
        void Undo();
    }
    public sealed class DelegateCommand : ICommand
    {
        private readonly Action execute, undo;
        private readonly Func<bool> canExecute;
        public DelegateCommand(Action execute, Action undo, Func<bool> canExecute = null)
        { this.execute = execute ?? throw new ArgumentNullException(nameof(execute)); this.undo = undo ?? throw new ArgumentNullException(nameof(undo)); this.canExecute = canExecute; }
        public bool CanExecute => canExecute?.Invoke() ?? true;
        public void Execute() => execute();
        public void Undo() => undo();
    }
    public sealed class CompositeCommand : ICommand
    {
        private readonly ICommand[] commands;
        public CompositeCommand(params ICommand[] commands)
        {
            this.commands = (ICommand[])(commands ?? throw new ArgumentNullException(nameof(commands))).Clone();
            foreach (var command in this.commands) if (command == null) throw new ArgumentException("Null command.");
        }
        public bool CanExecute { get { foreach (var command in commands) if (!command.CanExecute) return false; return true; } }
        public void Execute()
        {
            if (!CanExecute) throw new InvalidOperationException("Composite command cannot execute.");
            int completed = 0;
            try { for (; completed < commands.Length; completed++) commands[completed].Execute(); }
            catch (Exception original)
            {
                var errors = new List<Exception> { original };
                for (int i = completed - 1; i >= 0; i--) try { commands[i].Undo(); } catch (Exception error) { errors.Add(error); }
                if (errors.Count > 1) throw new AggregateException(errors); throw;
            }
        }
        public void Undo() { for (int i = commands.Length - 1; i >= 0; i--) commands[i].Undo(); }
    }
    // Commands must be atomic when throwing; history commits only after a successful operation.
    public sealed class CommandHistory
    {
        private readonly List<ICommand> undo = new List<ICommand>(), redo = new List<ICommand>();
        private readonly int capacity;
        private bool executing;
        public int UndoCount => undo.Count;
        public int RedoCount => redo.Count;
        public CommandHistory(int capacity = 64) { if (capacity < 1) throw new ArgumentOutOfRangeException(nameof(capacity)); this.capacity = capacity; }
        public bool Execute(ICommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            Begin();
            try
            {
                if (!command.CanExecute) return false;
                command.Execute(); undo.Add(command); if (undo.Count > capacity) undo.RemoveAt(0); redo.Clear(); return true;
            }
            finally { executing = false; }
        }
        public bool Undo()
        {
            Begin(); try { if (undo.Count == 0) return false; var command = undo[undo.Count - 1]; command.Undo(); undo.RemoveAt(undo.Count - 1); redo.Add(command); return true; }
            finally { executing = false; }
        }
        public bool Redo()
        {
            Begin(); try { if (redo.Count == 0) return false; var command = redo[redo.Count - 1]; if (!command.CanExecute) return false; command.Execute(); redo.RemoveAt(redo.Count - 1); undo.Add(command); return true; }
            finally { executing = false; }
        }
        public void Clear() { if (executing) throw new InvalidOperationException("History is executing."); undo.Clear(); redo.Clear(); }
        private void Begin() { if (executing) throw new InvalidOperationException("Recursive command execution."); executing = true; }
    }
}
