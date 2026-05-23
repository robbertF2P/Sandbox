/// <reference path="..\App/Expense.js" />
/// <reference path="..\App/ExpenseItem.js" />

describe("Expense objects", function () {
    it("should be of type ExpenseItem", function () {
        var expenseItem = new ExpenseItem();
        var expense = new Expense(expenseItem);
        expect(expense.expenseItem).toBe(expenseItem);
    });
});