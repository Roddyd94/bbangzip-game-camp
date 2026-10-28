add_library(StudyCore STATIC
  Object.cpp World.cpp WindowManager.cpp RenderManager.cpp CollisionManager.cpp GameEngine.cpp FileSerializer.cpp TimeManager.cpp InputComponent.cpp)
target_include_directories(StudyCore PUBLIC "${CMAKE_CURRENT_SOURCE_DIR}/..")
target_link_libraries(StudyCore PUBLIC study_graphics study_window study_system)
target_compile_features(StudyCore PUBLIC cxx_std_17)
target_compile_options(StudyCore PRIVATE /W4 /utf-8)
