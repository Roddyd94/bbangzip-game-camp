cmake_minimum_required(VERSION 3.24)
project(StudyGame LANGUAGES CXX)

if(NOT WIN32 OR NOT MSVC OR NOT CMAKE_SIZEOF_VOID_P EQUAL 8)
    message(FATAL_ERROR "Bundled SFML requires Windows x64 and MSVC.")
endif()

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)
set(CMAKE_RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin/$<CONFIG>")

# Local SFML 3.1.0 package: no download required.
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

add_subdirectory(src/core)

# StudyCore and main are supplied. Only exercise sources are selectable.
set(STUDY_TODO_SOURCES "" CACHE STRING "Semicolon-separated src/todo cpp files; empty selects all")
if(STUDY_TODO_SOURCES)
    set(todo_sources ${STUDY_TODO_SOURCES})
else()
    file(GLOB_RECURSE todo_sources CONFIGURE_DEPENDS "${CMAKE_CURRENT_SOURCE_DIR}/src/todo/*.cpp")
endif()

file(REAL_PATH "${CMAKE_CURRENT_SOURCE_DIR}/src/todo" todo_root)
foreach(source IN LISTS todo_sources)
    if(NOT EXISTS "${source}")
        message(FATAL_ERROR "Todo source does not exist: ${source}")
    endif()
    file(REAL_PATH "${source}" source_path)
    cmake_path(IS_PREFIX todo_root "${source_path}" NORMALIZE inside_todo)
    if(NOT inside_todo OR NOT source_path MATCHES "\\.cpp$")
        message(FATAL_ERROR "Select only .cpp files inside src/todo: ${source}")
    endif()
endforeach()

add_executable(StudyGame src/main.cpp ${todo_sources})

target_link_libraries(StudyGame PRIVATE StudyCore)
target_compile_options(StudyGame PRIVATE /W4 /utf-8)

# Copy only the DLLs for the selected build configuration.
foreach(module system window graphics)
    add_custom_command(TARGET StudyGame POST_BUILD
        COMMAND ${CMAKE_COMMAND} -E copy_if_different
            $<TARGET_FILE:study_${module}>
            $<TARGET_FILE_DIR:StudyGame>
        VERBATIM)
endforeach()

# Keep data editable independently of source and copy it on every build.
add_custom_target(StudyContent ALL
    COMMAND ${CMAKE_COMMAND} -E copy_directory
        "${CMAKE_CURRENT_SOURCE_DIR}/content"
        "$<TARGET_FILE_DIR:StudyGame>/content"
    VERBATIM)
add_dependencies(StudyGame StudyContent)
