Step-by-step: Deplying Ghub RGB Agent on your Windows 11 machine

1. Build the Ghub RGB Agent executable using the provided source code and instructions.
2. Publish the executable to a location on your Windows 11 machine (e.g., C:\Path\To\GhubRgbAgent.exe. Use the command: dotnet publish -c Release -r win-x64 --self-contained true -o C:\Path\To\GhubRgbAgent).
3. Follow the steps below to create a Scheduled Task that will run the Ghub RGB Agent at logon.

Step‑by‑step: Create the Scheduled Task

1. Open Task Scheduler by searching for it in the Start menu. Type: taskschd.msc
2. Create a new task (NOT “Basic Task”). Click Create Task… (right panel). 
3. Go to General tab. Name: Ghub RGB Agent. Un-Check “Run with highest privileges”. Configure for: Windows 11. Run only when user is logged on. Leave “Hidden” unchecked (you can hide later if desired).
4. Go to Triggers tab. Click New… Begin the task: At log on. Settings: Specific User - enter your username. Click OK. Enabled = OK.
5. Go to Actions tab. Click New… Action: Start a program. Program/script: Browse to the location of your Ghub RGB Agent executable (e.g., C:\Path\To\GhubRgbAgent.exe). Click OK. Start in: (optional) you can specify the directory where the executable is located. Click OK. Enabled = OK.
6. Go to Conditions tab. Uncheck “Start the task only if the computer is on AC power” (if you want it to run on battery). Adjust any other conditions as needed. Click OK.
7. Go to Settings tab. Adjust settings as needed (e.g.,Check - allow task to be run on demand, Check - Run task as soon as possible after a scheduled start is missed, Set Attempt to restart every: 1 minute, Set Attempt to restart up to: 3 times, Uncheck - Stop task if it runs longer than, Check - if the task does not end when requested, Uncheck - if the task is not scheduled to run again, Select - Do not start a new instance). Click OK.
8. Review your task in the Task Scheduler Library to ensure it’s set up correctly. You should see your Ghub RGB Agent task listed there.
9. Test the task by right-clicking on it and selecting “Run”. Ensure that the Ghub RGB Agent starts as expected. You can also check the Task Scheduler history for any errors or issues with the task execution.
10. Once confirmed that the task is working correctly, it will automatically start the Ghub RGB Agent every time you log in to your Windows 11 account.