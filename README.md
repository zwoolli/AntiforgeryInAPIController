# Antiforgery
How to perform integration test on ASP.NET Core API controller endpoint that requires authentication and validation of antiforgery tokens

# Description
I have an ASP.NET Core API controller endpoint that requires:

 - an authenticated user, and
 - validation of antiforgery tokens

I want to perform an integration test on this endpoint.

# Problem
I am unable to send a request that has both an authenticated user and the necessary antiforgery tokens/cookies and authentication cookies. Therefore, the endpoint continues to return a `Bad Request` before reaching the handler. 
# Code

# Question