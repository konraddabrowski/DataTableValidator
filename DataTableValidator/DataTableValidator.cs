using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace ExpressionValidator
{
    public class DataTableValidator
    {
        private DataTable _table;
        private Dictionary<string, object> _row;
        public bool _isValidationCalled = false;
        public bool IsValid { get; protected set; } = false;

        /// <summary>
        /// Create new Instance of DataTableValidator
        /// </summary>
        /// <param name="columnNames">Tables column names</param>
        /// <param name="row">Row data (should has the same column names as the table)</param>
        public DataTableValidator(IEnumerable<string> columnNames, Dictionary<string, object> row)
        {
            SetRow(row);
            SetTable(columnNames);
        }

        /// <summary>
        /// Set _row instance
        /// </summary>
        /// <param name="row">String - column name, object - value</param>        
        private void SetRow(Dictionary<string, object> row)
        {
            if (row is null)
            {
                throw new ArgumentNullException("The row field is undefined.");
            }
            if (!row.Any())
            {
                throw new ArgumentException("The row field is empty.");
            }

            _row = row;
        }

        /// <summary>
        /// Set _table instance
        /// </summary>
        /// <param name="columnNames">Names of columns</param>
        private void SetTable(IEnumerable<string> columnNames)
        {
            _table = new DataTable();
            if (columnNames is null)
            {
                throw new ArgumentNullException("The columns field is undefined.");
            }
            if (!columnNames.Any())
            {
                throw new ArgumentException("The columns field is empty.");
            }

            columnNames.ToList().ForEach(x => _table.Columns.Add(new DataColumn(x)));
        }

        /// <summary>
        /// Validate the row columns for the table
        /// </summary>
        /// <returns>Validation result</returns>
        public ValidationResult Validate()
        {
            _isValidationCalled = true;
            if (_row.Count != _table.Columns.Count)
            {
                return new ValidationResult("The amount of columns row does not match the amount of columns table.");
            }

            var columnErrors = new List<string>();
            foreach (var column in _row)
            {
                if(!_table.Columns.Contains(column.Key))
                {
                    columnErrors.Add(column.Key);
                }
            }

            if (columnErrors.Any())
            {
                return new ValidationResult($"The row has invalid column names. See invalid columns in MemberNames property.", columnErrors);
            }
            
            IsValid = true;
            return ValidationResult.Success;
        }

        /// <summary>
        /// Estimate expression 
        /// </summary>
        /// <param name="expression">Sql where expression</param>
        /// <returns>Validation result</returns>
        public ValidationResult Estimate(string expression)
        {
            if (string.IsNullOrEmpty(expression))
            {
                return new ValidationResult("The expression can not be empty.");
            }

            try
            {
                PopulateTable();
                _table.Columns.Add(new DataColumn("Expression", typeof(string), expression));
            }
            catch (ArgumentException ex)
            {
                return new ValidationResult(ex.Message);
            }
            catch (MissingMethodException ex)
            {
                return new ValidationResult(ex.Message);
            }
            catch (Exception)
            {
                return new ValidationResult("Incorrect expression.");
            }

            return ValidationResult.Success;

        }

        /// <summary>
        /// Populate table of one row
        /// </summary>
        private void PopulateTable()
        {
            if (!_isValidationCalled)
            {
                throw new MissingMethodException("The validation method has been not run.");
            }
            if (!IsValid)
            {
                throw new ArgumentException("The validation failed.");
            }

            var row = _table.NewRow();
            _row.Keys.ToList().ForEach(x => row[x] = _row[x]);
            _table.Rows.Add(row);
        }
    }
}
