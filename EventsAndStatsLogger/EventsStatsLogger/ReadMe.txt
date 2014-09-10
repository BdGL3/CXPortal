For Vista/7 an error message of

HTTP could not register URL http://+:8888/. Your process does not have access rights to
this namespace (see http://go.microsoft/com/fwlink/?LinkId=70353 for details).

This is because the HTTP namespace has not been this url and thus denies access.
This can be fixed 1 of 2 ways.

1.  Run the program as Administration (Not advised)
2.  Add the http to the namespace by
		- Open Command prompt as Run as Administrator
		- type "netsh http show urlacl"
			- This will show all the allowed http connections
		- type "netsh http add urlacl url=http://+:8888/ user=\Everyone"
		- type "netsh http show urlacl"
			- Verify that the new http entry shows up