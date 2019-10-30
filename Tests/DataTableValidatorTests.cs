using System;
using System.Collections.Generic;
using ExpressionValidator;
using FluentAssertions;
using NUnit.Framework;

namespace Tests
{
    public class ValidateMethod
    {
        private IEnumerable<string> _columns;
        private Dictionary<string, object> _row;
         
        [SetUp]
        public void Setup()
        {
            _columns = new List<string> {
                "Name",
                "Type",
                "Description"
            };
            _row = new Dictionary<string, object>{
                {"Name", "textName"},
                {"Type", 2},
                {"Description", "textDescription"},
            };
        }

        [Test]
        public void Passing_undefined_columns_to_constructor_DataTableValidator_should_throw_argument_exception()
        {
            Action action = () => new DataTableValidator(null, _row);

            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Passing_empty_columns_to_constructor_DataTableValidator_should_throw_argument_exception()
        {
            Action action = () => new DataTableValidator(new List<string>(), _row);

            action.Should().Throw<ArgumentException>().WithMessage("The columns field is empty.");
        }

        [Test]
        public void Passing_undefined_data_to_constructor_DataTableValidator_should_throw_argument_exception()
        {
            Action action = () => new DataTableValidator(_columns, null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Passing_empty_row_to_constructor_DataTableValidator_should_throw_argument_exception()
        {
            Action action = () => new DataTableValidator(_columns, new Dictionary<string, object>());

            action.Should().Throw<ArgumentException>().WithMessage("The row field is empty.");
        }
        
        [Test]
        public void Passing_different_count_of_columns_and_row_validate_method_should_return_validation_result_with_error_message()
        {
            var columns = new List<string> {
                "Name",
                "Type"
            };

            var dtv = new DataTableValidator(columns, _row);
            var validateResult = dtv.Validate();

            validateResult.ErrorMessage.Should().BeEquivalentTo("The amount of columns row does not match the amount of columns table.");
        }
        
        [Test]
        public void Passing_invalid_row_column_name_validate_method_should_return_validation_result_with_error_message_containing_reasons()
        {
            var row = new Dictionary<string, object>{
                {"Name", "textName"},
                {"Type", 2},
                {"Opis", "textOpis"},
            };

            var dtv = new DataTableValidator(_columns, row);
            var validateResult = dtv.Validate();

            validateResult.ErrorMessage
                .Should()
                .BeEquivalentTo("The row has invalid column names. See invalid columns in MemberNames property.");
            validateResult.MemberNames.Should().BeEquivalentTo(new string [] {"Opis"});
        }

        [Test]
        public void Passing_undefined_expression_estimate_method_should_return_validation_result_with_error_message()
        {
            var dtv = new DataTableValidator(_columns, _row);
            dtv.Validate();

            var result = dtv.Estimate(null);

            result.ErrorMessage.Should().BeEquivalentTo("The expression can not be empty.");
        }

        [Test]
        public void Passing_empty_expression_estimate_method_should_return_validation_result_with_error_message()
        {
            var dtv = new DataTableValidator(_columns, _row);
            dtv.Validate();
            
            var result = dtv.Estimate("");

            result.ErrorMessage.Should().BeEquivalentTo("The expression can not be empty.");
        }

        [Test]
        public void Passing_valid_expresion_without_validate_row_estimate_method_should_return_validation_result_with_error_message()
        {
            var dtv = new DataTableValidator(_columns, _row);
            
            var result = dtv.Estimate("Name = textName");

            result.ErrorMessage.Should().BeEquivalentTo("The validation method has been not run.");
        }

        [Test]
        public void Passing_valid_expresion_with_invalidate_row_estimate_method_should_return_validation_result_with_error_message()
        {
            var row = new Dictionary<string, object>{
                {"Name", "textName"}
            };
            var dtv = new DataTableValidator(_columns, row);
            dtv.Validate();
            
            var result = dtv.Estimate("Name = textName");

            result.ErrorMessage.Should().BeEquivalentTo("The validation failed.");
        }

        [Test]
        public void Passing_valid_expresion_estimate_method_should_return_validation_result_with_success()
        {
            var expression = "Name = \'textName\' and Type = 2";
            var dtv = new DataTableValidator(_columns, _row);
            
            dtv.Validate();
            var result = dtv.Estimate(expression);

            // ValidtionResult.Success is equal null
            result.Should().BeNull();
        }
    }
}