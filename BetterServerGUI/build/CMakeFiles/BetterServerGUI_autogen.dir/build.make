# CMAKE generated file: DO NOT EDIT!
# Generated by "Unix Makefiles" Generator, CMake Version 3.16

# Delete rule output on recipe failure.
.DELETE_ON_ERROR:


#=============================================================================
# Special targets provided by cmake.

# Disable implicit rules so canonical targets will work.
.SUFFIXES:


# Remove some rules from gmake that .SUFFIXES does not remove.
SUFFIXES =

.SUFFIXES: .hpux_make_needs_suffix_list


# Suppress display of executed commands.
$(VERBOSE).SILENT:


# A target that is always out of date.
cmake_force:

.PHONY : cmake_force

#=============================================================================
# Set environment variables for the build.

# The shell in which to execute make rules.
SHELL = /bin/sh

# The CMake executable.
CMAKE_COMMAND = /usr/bin/cmake

# The command to remove a file.
RM = /usr/bin/cmake -E remove -f

# Escaping for special characters.
EQUALS = =

# The top-level source directory on which CMake was run.
CMAKE_SOURCE_DIR = /home/hander/disasterserver/BetterServerGUI

# The top-level build directory on which CMake was run.
CMAKE_BINARY_DIR = /home/hander/disasterserver/BetterServerGUI/build

# Utility rule file for BetterServerGUI_autogen.

# Include the progress variables for this target.
include CMakeFiles/BetterServerGUI_autogen.dir/progress.make

CMakeFiles/BetterServerGUI_autogen:
	@$(CMAKE_COMMAND) -E cmake_echo_color --switch=$(COLOR) --blue --bold --progress-dir=/home/hander/disasterserver/BetterServerGUI/build/CMakeFiles --progress-num=$(CMAKE_PROGRESS_1) "Automatic MOC and UIC for target BetterServerGUI"
	/usr/bin/cmake -E cmake_autogen /home/hander/disasterserver/BetterServerGUI/build/CMakeFiles/BetterServerGUI_autogen.dir/AutogenInfo.json ""

BetterServerGUI_autogen: CMakeFiles/BetterServerGUI_autogen
BetterServerGUI_autogen: CMakeFiles/BetterServerGUI_autogen.dir/build.make

.PHONY : BetterServerGUI_autogen

# Rule to build all files generated by this target.
CMakeFiles/BetterServerGUI_autogen.dir/build: BetterServerGUI_autogen

.PHONY : CMakeFiles/BetterServerGUI_autogen.dir/build

CMakeFiles/BetterServerGUI_autogen.dir/clean:
	$(CMAKE_COMMAND) -P CMakeFiles/BetterServerGUI_autogen.dir/cmake_clean.cmake
.PHONY : CMakeFiles/BetterServerGUI_autogen.dir/clean

CMakeFiles/BetterServerGUI_autogen.dir/depend:
	cd /home/hander/disasterserver/BetterServerGUI/build && $(CMAKE_COMMAND) -E cmake_depends "Unix Makefiles" /home/hander/disasterserver/BetterServerGUI /home/hander/disasterserver/BetterServerGUI /home/hander/disasterserver/BetterServerGUI/build /home/hander/disasterserver/BetterServerGUI/build /home/hander/disasterserver/BetterServerGUI/build/CMakeFiles/BetterServerGUI_autogen.dir/DependInfo.cmake --color=$(COLOR)
.PHONY : CMakeFiles/BetterServerGUI_autogen.dir/depend

