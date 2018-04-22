# Usage
## Create Flow
1. Create a new blank flow.

2. Add a Schedule trigger.  
![](./ReportRenderer_Flow_Schedule.png "Schedule Trigger")

3. Add an Initialize Variable action.  
![](./ReportRenderer_Flow_Variable.png "Initialize Variable Action")


4. Add the Render Report action, select a report and select a report from the list.   You must convert the Parameters variable to a string using an expression of `string(variables('Parameters'))`.  
![](./ReportRenderer_Flow_RenderReport.png "Render Report Action")

5. Add a Send an Email action.

6. Under advanced options, add an attachment.  You must add an expression to convert Base64 to binary.  
   The expression should be `base64ToBinary(body('Render_Report')?['Output'])`  
![](./ReportRenderer_Flow_Content.png "Content Expression")

7. Save and test the flow.