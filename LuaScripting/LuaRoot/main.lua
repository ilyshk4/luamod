-- Created 07/26/2021 16:26:35 

local machine_info = machine.get_machine_info()
local starting_block = machine_info.get_block_info(0)

local function play()
  -- called on simulation start
end

local function update()
  -- called every frame
end

local function late_update()
  -- called every frame after update
end

local function fixed_update()
  -- frame-rate independent update (called 100 times per second)
end

local function on_gui()
  -- called for rendering and handling GUI events
end

return {
  play = play,
  update = update,
  late_update = late_update,
  fixed_update = fixed_update,
  on_gui = on_gui,
}

