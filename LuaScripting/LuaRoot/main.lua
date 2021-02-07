-- Created 01/10/2021 16:18:15 

local machine_info = machine.get_machine_info() -- lua table of functions for getting machine info or block info

chat.set_visible(true)

local r1 = machine.get_refs_control("r1") -- get controller of all blocks with reference key 'r1'
local b1 = machine_info.get_block_info("b1") -- get block info with reference key 'b1'
local s1 = 1 -- speed variable

local r2 = machine.get_refs_control("r2")
local b2 = machine_info.get_block_info("b2")
local s2 = 1

local r3 = machine.get_refs_control("r3")
local b3 = machine_info.get_block_info("b3")
local s3 = 1

local r4 = machine.get_refs_control("r4")
local b4 = machine_info.get_block_info("b4")
local s4 = 1

local window_rect = rect.new(100, 450, 400, 200) -- gui window rectangle (see unity scripting api about gui)

local target_height = machine_info.position().y
local rotor_speed = 1
local max_rotor_boost = 0.5


local function play()
end

local function update()

end

local function late_update()
end

local function clamp(value, min, max)
  if value <= min then
    value = min
  end
    if value >= max then
    value = max
  end 
  return value
end

local function cool_slider(text, value, min, max, order)
  value = gui.horizontal_slider(rect.new(20, 30 * order, 360, 30), value, min, max )
  gui.label(rect.new(20, 30 * order + 10, 360, 30), text .. value)
  return value
end

local function rotor_update(r, b, s)
  local height_diff = target_height - b.position().y
  local velocity = b.velocity().y
  
  s = s - velocity / 1000
  
  if math.abs(height_diff) > 3 then
    s = s + clamp(height_diff, -max_rotor_boost, max_rotor_boost) / 100
  end
  
  r.set_slider("speed", s) -- set slider with key 'speed' value
  
  return s
end

local function fixed_update()
  s1 = rotor_update(r1, b1, s1)
  s2 = rotor_update(r2, b2, s2)
  s3 = rotor_update(r3, b3, s3)
  s4 = rotor_update(r4, b4, s4)

end

local function controller_window()
  target_height = cool_slider("Target Height: ", target_height, -100, 100, 1)
  max_rotor_boost = cool_slider("Max Rotors Boost: ", max_rotor_boost, 0, 5, 2)
  
  gui.drag_window()
end

local function on_gui()
  window_rect = gui.window(1000, window_rect, "Drone Control Panel", controller_window) -- draw unity gui window
end

return {
  play = play,
  update = update,
  late_update = late_update,
  fixed_update = fixed_update,
  on_gui = on_gui,
}

