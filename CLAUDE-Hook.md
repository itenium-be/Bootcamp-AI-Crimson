Claude Hooks
============

Like `git hooks`, but for Claude.

Notification Hook
-----------------

You want this one?
Just point Claude to this file, and I'm sure it can do it for you!

### Install

If Claude needs your attention, get a notification!

Inside your `~/.claude/settings.json`, add the following.

```json
"hooks": {
  "Notification": [
    {
      "matcher": "",
      "hooks": [
        {
          "type": "command",
          "command": "powershell -NoProfile -ExecutionPolicy Bypass -File C:/Users/_username_/.claude/scripts/notify.ps1"
        }
      ]
    }
  ]
}
```


The `notify.ps1`:

```ps1
$json = [Console]::In.ReadToEnd() | ConvertFrom-Json
$title = if ($json.title) { $json.title } else { "Claude Code" }
$msg   = if ($json.message) { $json.message } else { "Waiting for your input" }

Add-Type -AssemblyName System.Windows.Forms
$n = New-Object System.Windows.Forms.NotifyIcon
$n.Icon    = [System.Drawing.SystemIcons]::Information
$n.Visible = $true
$n.ShowBalloonTip(10000, $title, $msg, [System.Windows.Forms.ToolTipIcon]::Info)
Start-Sleep -Seconds 11
$n.Dispose()
```
