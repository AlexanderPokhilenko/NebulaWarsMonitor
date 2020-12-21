Feature: Authorization
	As a user of monitor
	I want to log in

@WrongPassword
Scenario: Wrong password
	Given that user is on the "Monitor/LogIn" page
	When user enters wrong password "wrong_password"
	And user click the "send" button
	Then the "error" message should be shown

@CorrectPassword
Scenario: Correct password
	Given that user is on the "Monitor/LogIn" page
	When user enters correct password
	And user click the "send" button
	Then the user will be redirected to "Monitor/Home" page