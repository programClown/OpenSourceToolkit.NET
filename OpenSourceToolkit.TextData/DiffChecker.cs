using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;

namespace OpenSourceToolkit.TextData
{
    public class DiffChecker
    {
        private readonly ISideBySideDiffBuilder _diffBuilder;

        public DiffChecker()
        {
            _diffBuilder = new SideBySideDiffBuilder(new Differ());
        }

        public SideBySideDiffModel Compare(string oldText, string newText)
        {
            return _diffBuilder.BuildDiffModel(oldText, newText);
        }
    }
}
