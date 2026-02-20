const fs = require("fs")
const https = require("https")
const path = require("path")

// Define all components to fetch
const components = [
  {
    name: "EnhancedNexus",
    url: "https://uxcanvas.ai/api/projects/300ccbb5-5157-4041-b7df-4cb9ecbd2fed/25/code/EnhancedNexus",
    filePath: "src/components/EnhancedNexus/index.tsx",
    description:
      "Enhanced central command nexus with dockable container, module loading, orbital icons, advanced interactions, and proper dock button integration",
  },
  {
    name: "AdvancedDraggableModule",
    url: "https://uxcanvas.ai/api/projects/300ccbb5-5157-4041-b7df-4cb9ecbd2fed/25/code/AdvancedDraggableModule",
    filePath: "src/components/AdvancedDraggableModule/index.tsx",
    description:
      "Advanced draggable module with drop zones, docking previews, frosted glass effects, and enhanced accessibility",
  },
  {
    name: "EnergyFlow",
    url: "https://uxcanvas.ai/api/projects/300ccbb5-5157-4041-b7df-4cb9ecbd2fed/25/code/EnergyFlow",
    filePath: "src/components/EnergyFlow/index.tsx",
    description: "Animated energy flow component for spaceship-style power circuits",
  },
  {
    name: "CommandNexus",
    url: "https://uxcanvas.ai/api/projects/300ccbb5-5157-4041-b7df-4cb9ecbd2fed/25/code/CommandNexus",
    filePath: "src/components/CommandNexus/index.tsx",
    description: "Central command prompt nexus with AI prompt input and contextual panels",
  },
  {
    name: "DraggableModule",
    url: "https://uxcanvas.ai/api/projects/300ccbb5-5157-4041-b7df-4cb9ecbd2fed/25/code/DraggableModule",
    filePath: "src/components/DraggableModule/index.tsx",
    description: "A draggable AI module component with holographic effects and docking capabilities",
  },
  {
    name: "CognitiveMeshDashboard",
    url: "https://uxcanvas.ai/api/projects/300ccbb5-5157-4041-b7df-4cb9ecbd2fed/25/code/CognitiveMeshDashboard",
    filePath: "src/app/page.tsx",
    description:
      "A revolutionary spaceship-grade AI dashboard for the Cognitive Mesh Enterprise AI Transformation Framework",
  },
]

// Function to fetch a single component
function fetchComponent(component) {
  return new Promise((resolve, reject) => {
    console.log(`\n🚀 Fetching ${component.name}...`)
    console.log(`📝 Description: ${component.description}`)
    console.log(`🔗 URL: ${component.url}`)

    https
      .get(component.url, (res) => {
        console.log(`📡 Response status for ${component.name}: ${res.statusCode}`)

        let data = ""

        res.on("data", (chunk) => {
          data += chunk
        })

        res.on("end", () => {
          console.log(`📦 Data received for ${component.name}, length: ${data.length}`)

          const dir = path.dirname(component.filePath)

          // Create directory if it doesn't exist
          if (!fs.existsSync(dir)) {
            fs.mkdirSync(dir, { recursive: true })
            console.log(`📁 Created directory: ${dir}`)
          }

          try {
            fs.writeFileSync(component.filePath, data)
            console.log(`✅ ${component.name} saved successfully to: ${component.filePath}`)

            // Show first 150 characters of the fetched code
            console.log(`📄 Preview: ${data.substring(0, 150)}...`)

            resolve(component.name)
          } catch (error) {
            console.error(`❌ Error writing ${component.name}:`, error.message)
            reject(error)
          }
        })
      })
      .on("error", (err) => {
        console.error(`❌ Error fetching ${component.name}:`, err.message)
        reject(err)
      })
  })
}

// Main execution function
async function fetchAllComponents() {
  console.log("🌟 Starting to fetch all Cognitive Mesh components...")
  console.log(`📊 Total components to fetch: ${components.length}`)

  const results = {
    successful: [],
    failed: [],
  }

  // Use forEach to iterate through all components
  for (const component of components) {
    try {
      await fetchComponent(component)
      results.successful.push(component.name)
    } catch (error) {
      results.failed.push({
        name: component.name,
        error: error.message,
      })
    }
  }

  // Summary report
  console.log("\n" + "=".repeat(60))
  console.log("📋 FETCH SUMMARY REPORT")
  console.log("=".repeat(60))

  console.log(`✅ Successfully fetched: ${results.successful.length}/${components.length}`)
  results.successful.forEach((name) => {
    console.log(`   ✓ ${name}`)
  })

  if (results.failed.length > 0) {
    console.log(`\n❌ Failed to fetch: ${results.failed.length}/${components.length}`)
    results.failed.forEach((item) => {
      console.log(`   ✗ ${item.name}: ${item.error}`)
    })
  }

  console.log("\n🎉 Component fetching process completed!")

  if (results.successful.length === components.length) {
    console.log("🚀 All components ready! Your Cognitive Mesh Dashboard is fully loaded.")
  } else {
    console.log("⚠️  Some components failed to fetch. Fallback components will be used.")
  }
}

// Execute the script
fetchAllComponents().catch((error) => {
  console.error("💥 Fatal error in fetch process:", error)
})
