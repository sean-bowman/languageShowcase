// =============================================================================
// THREE.JS SCENE MANAGER
// =============================================================================
//
// GRAPHICS CONCEPT: 3D Scene Graph and Rendering Pipeline
// ========================================================
//
// Three.js is a JavaScript library that makes WebGL (OpenGL for the web)
// accessible. It provides a scene graph architecture:
//
// SCENE GRAPH HIERARCHY:
// ---------------------
//
//   Scene (root container)
//     |
//     +-- Lights
//     |     +-- AmbientLight (uniform lighting)
//     |     +-- DirectionalLight (sun simulation)
//     |     +-- PointLight (localized glow)
//     |
//     +-- Meshes (geometry + material)
//     |     +-- Earth sphere
//     |     +-- Spacecraft cone
//     |
//     +-- Lines (geometry only)
//           +-- Orbit paths
//           +-- Grid lines
//
// RENDERING PIPELINE:
// ------------------
//
//   1. Scene Graph           2. Camera Transform        3. Projection
//   (3D world coords)   -->  (view coords)         -->  (2D screen)
//
//   Objects positioned       Camera "looks" at          Perspective division
//   in world space           scene from a point         creates depth effect
//
// THREE.JS CORE OBJECTS:
// ---------------------
//   - Scene: Container for all 3D objects
//   - Camera: Viewpoint into the scene
//   - Renderer: Draws the scene to a canvas
//   - Mesh: Visible object (geometry + material)
//   - Light: Illumination source
//
// =============================================================================
// JAVASCRIPT CONCEPTS IN THIS FILE
// =============================================================================
//
// 1. ES6 CLASSES
//    `class SceneManager { constructor() { ... } }`
//    Modern syntax for creating objects with methods.
//
// 2. IMPORT STATEMENT
//    `import * as THREE from 'three';`
//    Imports all exports from a module as a namespace object.
//
// 3. ARROW FUNCTIONS FOR CALLBACKS
//    `window.addEventListener('resize', () => this.onResize());`
//    Arrow functions preserve `this` context from enclosing scope.
//
// 4. ARRAY METHODS
//    `this.animationCallbacks.forEach(callback => callback(delta))`
//    `push()`, `indexOf()`, `splice()` for array manipulation.
//
// 5. REQUESTANIMATIONFRAME
//    `requestAnimationFrame(() => this.animate());`
//    Browser API for smooth 60fps animations.
//
// 6. OBJECT DESTRUCTURING (implicit)
//    Options objects with defaults: `{ antialias: true }`
//
// =============================================================================

import * as THREE from 'three';
import { OrbitControls } from 'three/addons/controls/OrbitControls.js';

/**
 * Manages the Three.js scene, camera, renderer, and animation loop.
 *
 * @description
 * THREE.JS: The SceneManager encapsulates all the Three.js boilerplate:
 * - Scene setup (container for 3D objects)
 * - Camera configuration (viewpoint)
 * - WebGL renderer (draws to canvas)
 * - OrbitControls (mouse interaction)
 * - Animation loop (60fps updates)
 *
 * This class follows the "Facade" pattern - it provides a simple interface
 * to the complex Three.js subsystem.
 */
export class SceneManager {
    /**
     * Create a new scene manager.
     *
     * @description
     * JAVASCRIPT: The constructor is called when using `new SceneManager()`.
     * ES6 classes always have a constructor (explicit or implicit).
     *
     * The pattern of calling `this.init()` from the constructor is common
     * when initialization is complex or may need to be called again later.
     *
     * @param {HTMLElement} container - DOM element to render into
     */
    constructor(container) {
        // Store reference to container element
        this.container = container;

        // These will be initialized in init()
        // JAVASCRIPT: Explicit null initialization for clarity
        this.scene = null;
        this.camera = null;
        this.renderer = null;
        this.controls = null;

        // Array of callback functions to call each frame
        // JAVASCRIPT: Empty array literal initialization
        this.animationCallbacks = [];

        // Initialize all Three.js components
        this.init();
    }

    /**
     * Initialize the Three.js scene, camera, renderer, and controls.
     *
     * @description
     * THREE.JS: This is the standard initialization sequence:
     * 1. Create Scene (container for all objects)
     * 2. Create Camera (defines viewpoint and projection)
     * 3. Create Renderer (draws scene to canvas)
     * 4. Create Controls (user interaction)
     * 5. Setup Lighting (illumination)
     * 6. Start Animation Loop
     */
    init() {
        // =====================================================================
        // SCENE CREATION
        // =====================================================================
        // THREE.JS: The Scene is the root of the scene graph.
        // All objects must be added to the scene (or a child of the scene).
        this.scene = new THREE.Scene();

        // Set background color (dark blue for space)
        // THREE.JS: Colors can be specified as hex numbers
        this.scene.background = new THREE.Color(0x0a0a1a);

        // =====================================================================
        // CAMERA SETUP
        // =====================================================================
        // THREE.JS: PerspectiveCamera simulates human vision with depth
        // Parameters: (fieldOfView, aspectRatio, nearClip, farClip)
        //
        // GRAPHICS: Near/far clipping planes define the visible depth range.
        // Objects outside this range are not rendered (culled).
        const aspect = window.innerWidth / window.innerHeight;
        this.camera = new THREE.PerspectiveCamera(
            60,     // FOV in degrees (human eye ~ 60-90)
            aspect, // Width/height ratio
            0.1,    // Near plane (objects closer are clipped)
            10000   // Far plane (objects farther are clipped)
        );

        // Position camera and point it at origin
        // THREE.JS: position.set(x, y, z) sets 3D coordinates
        this.camera.position.set(0, 50, 80);
        this.camera.lookAt(0, 0, 0);

        // =====================================================================
        // RENDERER SETUP
        // =====================================================================
        // THREE.JS: WebGLRenderer uses the GPU for hardware-accelerated graphics
        this.renderer = new THREE.WebGLRenderer({
            antialias: true,  // Smooth edges (costs performance)
            alpha: true       // Transparent background support
        });

        // Match renderer size to window
        this.renderer.setSize(window.innerWidth, window.innerHeight);

        // Handle high-DPI displays (Retina, etc.)
        // JAVASCRIPT: Math.min prevents excessive resolution on high-DPI
        this.renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));

        // Add the canvas element to the DOM
        // THREE.JS: renderer.domElement is the <canvas> element
        this.container.appendChild(this.renderer.domElement);

        // =====================================================================
        // ORBIT CONTROLS
        // =====================================================================
        // THREE.JS ADDON: OrbitControls allows mouse-driven camera movement
        // - Left click + drag: rotate around target
        // - Right click + drag: pan
        // - Scroll: zoom in/out
        this.controls = new OrbitControls(this.camera, this.renderer.domElement);
        this.controls.enableDamping = true;    // Smooth camera movement
        this.controls.dampingFactor = 0.05;    // Damping strength
        this.controls.minDistance = 15;        // Closest zoom
        this.controls.maxDistance = 500;       // Farthest zoom

        // Setup lighting
        this.setupLighting();

        // Add reference grid
        this.addGrid();

        // Handle window resize events
        // JAVASCRIPT: Arrow function preserves `this` context
        window.addEventListener('resize', () => this.onResize());

        // Start the animation loop
        this.animate();
    }

    /**
     * Setup scene lighting.
     *
     * @description
     * THREE.JS: Lighting is essential for 3D depth perception.
     * Different light types serve different purposes:
     *
     * AMBIENT LIGHT:
     *   - Uniform illumination from all directions
     *   - Fills in shadows, provides base brightness
     *   - No direction, no shadows
     *
     * DIRECTIONAL LIGHT:
     *   - Parallel rays (simulates distant source like sun)
     *   - Has direction but no position (infinitely far)
     *   - Casts shadows (if enabled)
     *
     * POINT LIGHT:
     *   - Radiates from a point in all directions
     *   - Intensity decreases with distance (attenuation)
     *   - Like a light bulb
     */
    setupLighting() {
        // Ambient light for base illumination
        // Parameters: (color, intensity)
        const ambientLight = new THREE.AmbientLight(0x404040, 0.5);
        this.scene.add(ambientLight);

        // Directional light simulating the sun
        // Position determines direction (light points toward origin)
        const sunLight = new THREE.DirectionalLight(0xffffff, 1.0);
        sunLight.position.set(100, 50, 100);
        this.scene.add(sunLight);

        // Point light at center for dramatic effect on Earth
        // Parameters: (color, intensity, distance)
        // Distance = 0 means infinite range, otherwise light fades
        const centerLight = new THREE.PointLight(0x4a90d9, 0.5, 100);
        centerLight.position.set(0, 0, 0);
        this.scene.add(centerLight);
    }

    /**
     * Add a circular grid for spatial reference.
     *
     * @description
     * GRAPHICS: Reference grids help users understand scale and orientation.
     * A polar grid (concentric circles + radial lines) is appropriate for
     * orbital visualization since orbits are centered on a body.
     *
     * THREE.JS: BufferGeometry is the modern, efficient geometry class.
     * It stores vertex data in typed arrays for GPU efficiency.
     */
    addGrid() {
        // Create semi-transparent line material
        const gridMaterial = new THREE.LineBasicMaterial({
            color: 0x1e3a5f,
            transparent: true,
            opacity: 0.3
        });

        // Add concentric circles at various radii
        const radii = [10, 20, 30, 40, 50, 60, 70, 80];

        // JAVASCRIPT: forEach with arrow function for iteration
        radii.forEach(radius => {
            const segments = 64;  // Number of line segments per circle
            const geometry = new THREE.BufferGeometry();
            const positions = [];  // Will hold [x1,y1,z1, x2,y2,z2, ...]

            // Generate circle vertices
            for (let i = 0; i <= segments; i++) {
                const theta = (i / segments) * Math.PI * 2;
                // GRAPHICS: Parametric circle: x = r*cos(t), z = r*sin(t)
                // Y = 0 for a horizontal circle (XZ plane)
                positions.push(
                    radius * Math.cos(theta),  // X
                    0,                          // Y (horizontal plane)
                    radius * Math.sin(theta)   // Z
                );
            }

            // THREE.JS: BufferAttribute stores vertex data
            // Parameters: (array, itemSize) - itemSize=3 for xyz coordinates
            geometry.setAttribute(
                'position',
                new THREE.Float32BufferAttribute(positions, 3)
            );

            const circle = new THREE.Line(geometry, gridMaterial);
            this.scene.add(circle);
        });

        // Add radial lines (spokes)
        for (let i = 0; i < 12; i++) {
            const theta = (i / 12) * Math.PI * 2;
            const geometry = new THREE.BufferGeometry();

            // Line from origin to edge
            geometry.setAttribute('position', new THREE.Float32BufferAttribute([
                0, 0, 0,                                    // Start at origin
                80 * Math.cos(theta), 0, 80 * Math.sin(theta)  // End at edge
            ], 3));

            const line = new THREE.Line(geometry, gridMaterial);
            this.scene.add(line);
        }
    }

    /**
     * Handle window resize events.
     *
     * @description
     * THREE.JS: When the window resizes, we must update:
     * 1. Camera aspect ratio (prevents distortion)
     * 2. Renderer size (fills new window size)
     *
     * updateProjectionMatrix() recalculates the projection matrix
     * after changing camera parameters.
     */
    onResize() {
        const width = window.innerWidth;
        const height = window.innerHeight;

        // Update camera aspect ratio
        this.camera.aspect = width / height;
        this.camera.updateProjectionMatrix();

        // Update renderer size
        this.renderer.setSize(width, height);
    }

    /**
     * Register a callback to be called each animation frame.
     *
     * @description
     * JAVASCRIPT: The callback pattern allows external code to hook into
     * the animation loop without modifying this class.
     *
     * @param {Function} callback - Function to call with delta time
     */
    addAnimationCallback(callback) {
        // JAVASCRIPT: push() adds to end of array
        this.animationCallbacks.push(callback);
    }

    /**
     * Remove an animation callback.
     *
     * @param {Function} callback - The callback to remove
     */
    removeAnimationCallback(callback) {
        // JAVASCRIPT: indexOf returns -1 if not found
        const index = this.animationCallbacks.indexOf(callback);
        if (index > -1) {
            // JAVASCRIPT: splice(index, count) removes elements
            this.animationCallbacks.splice(index, 1);
        }
    }

    /**
     * Main animation loop.
     *
     * @description
     * JAVASCRIPT: requestAnimationFrame is the browser API for smooth animation.
     * It calls your function before the next screen repaint (~60 times/second).
     *
     * Benefits over setInterval:
     * - Syncs with display refresh rate
     * - Pauses when tab is hidden (saves CPU)
     * - Smoother animations
     *
     * THREE.JS: The render loop pattern:
     * 1. Update controls (camera movement)
     * 2. Update scene objects (animation)
     * 3. Render the scene
     * 4. Request next frame
     */
    animate() {
        // Schedule next frame
        // JAVASCRIPT: Arrow function preserves `this` binding
        requestAnimationFrame(() => this.animate());

        // Update orbit controls (smooth camera movement)
        this.controls.update();

        // Call all registered animation callbacks
        // Approximate delta time (assumes 60fps)
        const delta = 1 / 60;

        // JAVASCRIPT: forEach iterates over array
        this.animationCallbacks.forEach(callback => callback(delta));

        // Render the scene from the camera's viewpoint
        this.renderer.render(this.scene, this.camera);
    }

    /**
     * Add an object to the scene.
     *
     * @param {THREE.Object3D} object - The object to add
     */
    add(object) {
        this.scene.add(object);
    }

    /**
     * Remove an object from the scene.
     *
     * @param {THREE.Object3D} object - The object to remove
     */
    remove(object) {
        this.scene.remove(object);
    }

    /**
     * Reset camera to initial position.
     *
     * @description
     * THREE.JS: OrbitControls.reset() restores the camera to its
     * initial state (position, target, zoom).
     */
    resetCamera() {
        this.camera.position.set(0, 50, 80);
        this.camera.lookAt(0, 0, 0);
        this.controls.reset();
    }
}
