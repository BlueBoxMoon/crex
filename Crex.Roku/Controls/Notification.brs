rem --
rem -- init()
rem --
rem -- The Notification presents a horizontal bar on the top of the screen
rem -- which displays a notification message to the user.
rem --
sub init()
  rem --
  rem -- Set initial control values.
  rem --
  m.rBackground = m.top.findNode("rBackground")
  m.pImage = m.top.findNode("pImage")
  m.lMessage = m.top.findNode("lMessage")
  m.bDismiss = m.top.findNode("bDismiss")
  m.aSlide = m.top.FindNode("aSlide")
  m.aSlideValue = m.top.FindNode("aSlideValue")
  m.aSlideFadeValue = m.top.FindNode("aSlideFadeValue")

  config = ReadCache(m, "config")
  m.rBackground.color = "0x323232d0"
  m.aSlide.duration = config.AnimationTime
  m.top.state = "none"

  rem --
  rem -- Configure UI elements for the screen size we are running.
  rem --
  resolution = m.top.getScene().currentDesignResolution
  if resolution.resolution = "FHD"
    rem --
    rem -- Configure for 1920x1080.
    rem -- ratio 8:1
    m.rBackground.translation = [0, -225]
    m.rBackground.width = 1920
    m.rBackground.height = 225
    m.aSlideValue.keyValue = [[0, -225], [0, 0]]
    m.pImage.translation = [45, 45]
    m.pImage.width = 240
    m.pImage.height = 135
    m.lMessage.translation = [330, 45]
    m.lMessage.width = 1260
    m.lMessage.height = 135
    m.lMessage.font.size = 33
    m.lMessage.lineSpacing = 12
    m.bDismiss.translation = [1635, 134]
    m.bDismiss.width = 240
    m.bDismiss.height = 64
  else
    rem --
    rem -- Configure for 1280x720.
    rem --
    m.rBackground.translation = [0, -150]
    m.rBackground.width = 1280
    m.rBackground.height = 150
    m.aSlideValue.keyValue = [[0, -150], [0, 0]]
    m.pImage.translation = [30, 30]
    m.pImage.width = 160
    m.pImage.height = 90
    m.lMessage.translation = [220, 30]
    m.lMessage.width = 840
    m.lMessage.height = 90
    m.lMessage.font.size = 22
    m.lMessage.lineSpacing = 8
    m.bDismiss.translation = [1090, 78]
    m.bDismiss.width = 160
    m.bDismiss.height = 42
  end if

  rem --
  rem -- Observe the fields we need to monitor for changes.
  rem --
  m.top.observeField("focusedChild", "onFocusedChildChange")
  m.pImage.observeField("loadStatus", "onImageStatus")
  m.aSlide.observeField("state", "onSlideState")
end sub

rem *******************************************************
rem ** EVENT HANDLERS
rem *******************************************************

rem --
rem -- onNotificationChange()
rem --
rem -- Called when the notification state has changed.
rem --
sub onNotificationChange()
  m.lMessage.text = m.top.notification.Message
  if m.top.notification.Image = invalid
    m.pImage.visible = false
    m.aSlideValue.reverse = false
    m.aSlideFadeValue.reverse = false
    m.aSlide.control = "start"
    m.top.state = "visible"
  else
    m.pImage.uri = ""
    m.pImage.uri = BestMatchingUrl(m.top.notification.Image)
    m.pImage.visible = true
  end if
end sub

rem --
rem -- onImageStatus()
rem --
rem -- Called once the background image has finished loading. At this
rem -- point we can show the notification.
rem --
sub onImageStatus()
  if m.pImage.loadStatus = "ready" or m.pImage.loadStatus = "failed"
    m.aSlideValue.reverse = false
    m.aSlideFadeValue.reverse = false
    m.aSlide.control = "start"
    m.top.state = "visible"
  end if
end sub

rem --
rem -- onSlideState()
rem --
rem -- The animation for sliding the notification in or out has changed
rem -- state.
rem --
sub onSlideState()
  if m.aSlide.state = "stopped" and m.aSlideValue.reverse
    m.aSlide.control = "stop"
    m.top.state = "dismissed"
  end if
end sub

rem --
rem -- onFocusedChildChange()
rem --
rem -- Called when we gain focus. Ensure that the selected
rem -- button actually has the focus instead.
rem --
sub onFocusedChildChange()
  if m.top.IsInFocusChain() and not m.bDismiss.HasFocus()
    m.bDismiss.SetFocus(true)
  end if
end sub

rem --
rem -- onKeyEvent(key, press)
rem --
rem -- Called when the user presses a button on the remote. Check if we
rem -- need to handle any keys to change selection or activate their
rem -- current button.
rem --
rem -- @param key Contains the key that was pressed on the remote.
rem -- @param press True if the button was pressed, false if released.
rem -- @returns True if the key was handled, false otherwise.
rem --
function onKeyEvent(key as string, press as boolean) as boolean
  if press
    if key = "OK"
      m.aSlideValue.reverse = true
      m.aSlideFadeValue.reverse = true
      m.aSlide.control = "start"

      return true
    end if
  end if

  return false
end function
