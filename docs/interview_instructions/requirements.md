Programming Exercise

\-\-\-\-\-\-\-\-\-\-\-\-\-\-\-\-\-\-\-\-\-\-\-\-\-\-\-\-\-\--

 

Using Visual Studio or VS Code, write a small .NET console application
(using .NET 9 or .NET 10) that:

- Reads all spreadsheets from a folder (use the 3 attached Excel
  spreadsheets).
- For each file, output to the console the file name followed by each
  subrecipient name from that file. The subrecipient names will be under
  "G. Other Direct Costs" in the format "Subaward: {SubRecipientName}"
- Finally, output a distinct list of all subrecipients along with the
  total subaward amount that subrecipient received across all files. The
  output should be easy to read by our non-technical staff.

 

Requirements for the above .NET application:

- The app should work with [any spreadsheet in this
  format]{.underline} and be able to[ support a variable number of
  subaward rows]{.underline} (0 -- unknown).
- The app should have at least [one unit test]{.underline} defined that
  confirms there are 4 subrecipients "Indiana", "Mayo", "Purdue", and
  "Florida" in SubawardBudgetExample1.xlsx.
- The app should be [checked into a publicly accessible GitHub or like
  repository that the reviewers can pull and run]{.underline} (both the
  console app and unit tests), without any modification.
- In your README, list any assumptions you made about ambiguous
  requirements and any questions you would have asked us if this were
  real work.
- You may use AI assistants the way you would on the job --- as a tool,
  not a substitute. A follow-up interview will have you discuss the code
  you wrote and the reason behind your decisions. Please make sure you
  have a full understanding of what you submit.
