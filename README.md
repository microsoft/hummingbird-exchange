# Hummingbird for O365 and Active Directory
Sample that demonstrates how distribution groups can be converted to Office365 groups.

> **Use at your own risk.** This code is provided as-is with the sole intent of demonstrating EWS and AD APIs leveraged together to perform DL to Modern Group conversion.
> Neither groups nor distribution lists are automatically deleted.

##Where do I need to make changes?
Make sure that you specify your AD domain information (if you are using AD) in **Core/AppSetup.cs**. In addition to that, revise the built-in XML resources to properly reference your company domain.
