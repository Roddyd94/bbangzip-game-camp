cmake_minimum_required(VERSION 3.24)
project(StudyGame LANGUAGES CXX)
if(NOT WIN32 OR NOT MSVC OR NOT CMAKE_SIZEOF_VOID_P EQUAL 8)
  message(FATAL_ERROR "Bundled SFML binaries require Windows x64 MSVC.")
endif()
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(SFML_ROOT "${CMAKE_CURRENT_SOURCE_DIR}/third_party/SFML-3.1.0")
foreach(module system window graphics)
  add_library(study_${module} SHARED IMPORTED)
  set_target_properties(study_${module} PROPERTIES
    IMPORTED_CONFIGURATIONS "DEBUG;RELEASE"
    IMPORTED_IMPLIB_DEBUG "${SFML_ROOT}/lib/sfml-${module}-d.lib"
    IMPORTED_LOCATION_DEBUG "${SFML_ROOT}/bin/sfml-${module}-d-3.dll"
    IMPORTED_IMPLIB_RELEASE "${SFML_ROOT}/lib/sfml-${module}.lib"
    IMPORTED_LOCATION_RELEASE "${SFML_ROOT}/bin/sfml-${module}-3.dll"
    MAP_IMPORTED_CONFIG_RELWITHDEBINFO Release
    MAP_IMPORTED_CONFIG_MINSIZEREL Release
    INTERFACE_INCLUDE_DIRECTORIES "${SFML_ROOT}/include")
endforeach()
add_executable(StudyGame src/main.cpp src/GameCore.cpp)
target_link_libraries(StudyGame PRIVATE study_graphics study_window study_system)
target_compile_options(StudyGame PRIVATE /W4 /utf-8)
set_target_properties(StudyGame PROPERTIES RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin/$<CONFIG>")
foreach(module system window graphics)
  add_custom_command(TARGET StudyGame POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E copy_if_different
      $<TARGET_FILE:study_${module}> $<TARGET_FILE_DIR:StudyGame>)
endforeach()
enable_testing()
add_test(NAME StarterSmoke COMMAND StudyGame --smoke)
set_tests_properties(StarterSmoke PROPERTIES WORKING_DIRECTORY "$<TARGET_FILE_DIR:StudyGame>")
