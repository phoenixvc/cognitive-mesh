import DockZone from "@/components/DockZone";
import React from "react";

interface DashboardLayoutProps {
  dockHandleStyle: "grip" | "anchor" | "titlebar" | "ring" | "invisible";
}

const DashboardLayout: React.FC<DashboardLayoutProps> = ({ dockHandleStyle }) => {
  return (
    <>
      {/* Central Command Nexus Dock Zone */}
      <div className="flex justify-center mb-6">
        <DockZone
          id="central-nexus-dock"
          label="Command Center"
          maxItems={1}
          allowedSizes={["large"]}
          className="w-full max-w-2xl"
          isResizable={false}
          minWidth={400}
          minHeight={120}
          initialWidth={500}
          initialHeight={150}
          handleStyle={dockHandleStyle}
        />
      </div>

      {/* Metrics Dashboard */}
      <div className="grid grid-cols-1 lg:grid-cols-4 gap-6 mb-6">
        <DockZone
          id="metrics-dock"
          label="Metrics Dashboard"
          maxItems={4}
          allowedSizes={["small", "medium"]}
          className="lg:col-span-4"
          isResizable={true}
          minWidth={800}
          minHeight={180}
          initialWidth={1200}
          initialHeight={220}
          handleStyle={dockHandleStyle}
        />
      </div>

      {/* Main Content Dock Zones */}
      <div className="grid grid-cols-1 xl:grid-cols-3 gap-6 mb-6">
        <DockZone
          id="main-modules-dock"
          label="Main Modules"
          maxItems={6}
          allowedSizes={["medium", "large"]}
          className="xl:col-span-2"
          isResizable={true}
          minWidth={600}
          minHeight={400}
          initialWidth={800}
          initialHeight={500}
          handleStyle={dockHandleStyle}
        />

        <DockZone
          id="sidebar-dock"
          label="Sidebar Tools"
          maxItems={4}
          allowedSizes={["small", "medium"]}
          isResizable={true}
          minWidth={300}
          minHeight={400}
          initialWidth={400}
          initialHeight={500}
          handleStyle={dockHandleStyle}
        />
      </div>

      {/* Bottom Dock Zone */}
      <DockZone
        id="bottom-dock"
        label="Activity & Monitoring"
        maxItems={6}
        allowedSizes={["small", "medium", "large"]}
        isResizable={true}
        minWidth={800}
        minHeight={200}
        initialWidth={1200}
        initialHeight={300}
        handleStyle={dockHandleStyle}
      />
    </>
  );
};

export default DashboardLayout; 