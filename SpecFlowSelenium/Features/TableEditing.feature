Feature: Table editing
	As an authorized administrator of monitor
	I want to edit table of servers

@AddNewRow
Scenario: Adding new row
	Given that user is authorized
	And that user is on the "Monitor/Home" page
	When user click the "add" button
	Then the row with class "bg-success" will be in the table "mainTable"
	
@RemoveNewRow
Scenario: Removing new row
	Given that user is authorized
	And that user is on the "Monitor/Home" page
	When user click the "add" button
	And user click the button with class "removeButton" at the last row of table "mainTable"
	Then the row with class "bg-success" will not be in the table "mainTable"
	
@EditRow
Scenario: Editing row
	Given that user is authorized
	And that user is on the "Monitor/Home" page
	When user enters "any data" into first input of last row of table "mainTable"
	Then the row with class "bg-warning" will be in the table "mainTable"

@RemoveExistingRow
Scenario: Removing existing row
	Given that user is authorized
	And that user is on the "Monitor/Home" page
	When user click the button with class "removeButton" at the last row of table "mainTable"
	Then the row with class "bg-danger" will be in the table "mainTable"
	
@UndoRemovingRow
Scenario: Undoing removing row
	Given that user is authorized
	And that user is on the "Monitor/Home" page
	When user click the button with class "removeButton" at the last row of table "mainTable"
	And user click the button with class "removeButton" at the last row of table "mainTable"
	Then the row with class "bg-danger" will not be in the table "mainTable"