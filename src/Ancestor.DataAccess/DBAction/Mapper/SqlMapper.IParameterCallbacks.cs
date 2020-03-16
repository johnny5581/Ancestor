namespace Ancestor.DataAccess.DBAction.Mapper
{
    partial class SqlMapper
    {
        /// <summary>
        /// Extends IDynamicParameters with facilities for executing callbacks after commands have completed
        /// </summary>
        public interface IParameterCallbacks : IDynamicParameters
        {
            /// <summary>
            /// Invoked when the command has executed
            /// </summary>
            void OnCompleted();
        }
    }
}
